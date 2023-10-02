using Microsoft.Extensions.Logging;
using MrCapitalQ.EcoHive.EcoBee.Dtos;
using MrCapitalQ.EcoHive.EcoBee.Exceptions;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MrCapitalQ.EcoHive.EcoBee.Auth
{
    public class EcoBeePinAuthProvider : IEcoBeePinAuthProvider
    {
        private static readonly SemaphoreSlim s_semaphore = new(1);

        private readonly HttpClient _httpClient;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEcoBeeAuthCache _authCache;
        private readonly IEcoBeeRefreshTokenStore _refreshTokenStore;
        private readonly string _apiKey;
        private readonly ILogger<EcoBeePinAuthProvider> _logger;

        public EcoBeePinAuthProvider(HttpClient httpClient,
            IDateTimeProvider dateTimeProvider,
            IEcoBeeAuthCache authCache,
            IEcoBeeRefreshTokenStore refreshTokenStore,
            string apiKey,
            ILogger<EcoBeePinAuthProvider> logger)
        {
            _httpClient = httpClient;
            _dateTimeProvider = dateTimeProvider;
            _authCache = authCache;
            _refreshTokenStore = refreshTokenStore;
            _apiKey = apiKey;
            _logger = logger;
        }


        public async Task<PinData> GetPinAsync(string scope)
        {
            var url = $"https://api.ecobee.com/authorize?response_type=ecobeePin&client_id={_apiKey}&scope={scope}";
            var pinResponse = await _httpClient.GetFromJsonAsync<PinResponse>(url).ConfigureAwait(false)
                ?? throw new EcoBeeClientAuthException("Unexpected root literal null response when requesting a pin.");

            return new PinData
            {
                Pin = pinResponse.EcobeePin,
                AuthCode = pinResponse.Code,
                Scope = pinResponse.Scope,
                Expiration = _dateTimeProvider.UtcNow().AddMinutes(pinResponse.ExpiresIn)
            };
        }

        public async Task<bool> AuthenticateAsync(string authCode)
        {
            var authData = await RequestAccessTokenAsync(authCode).ConfigureAwait(false);

            return !string.IsNullOrEmpty(authData.AccessToken);
        }

        public async Task<AuthenticationHeaderValue?> GetAuthHeaderAsync(CancellationToken cancellationToken)
        {
            var authData = await _authCache.GetAsync().ConfigureAwait(false);

            if (authData is null
                && await _refreshTokenStore.GetAsync().ConfigureAwait(false) is var refreshToken
                && refreshToken is not null)
            {
                authData = await RefreshAccessTokenAsync(refreshToken).ConfigureAwait(false);
            }

            if (authData is not null)
                return new AuthenticationHeaderValue(authData.TokenType, authData.AccessToken);

            return null;
        }

        private async Task<EcoBeeAuthTokenData> RequestAccessTokenAsync(string authCode)
        {
            var url = $"https://api.ecobee.com/token?grant_type=ecobeePin&code={authCode}&client_id={_apiKey}&ecobee_type=jwt";
            var responseMessage = await _httpClient.PostAsJsonAsync(url, new object()).ConfigureAwait(false);
            var tokenResponse = await responseMessage.Content.ReadFromJsonAsync<TokenResponse>().ConfigureAwait(false)
                ?? throw new EcoBeeClientAuthException("Unexpected root literal null response when requesting an access token.");

            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                await _refreshTokenStore.SetAsync(tokenResponse.RefreshToken).ConfigureAwait(false);
            return await CacheAuthToken(tokenResponse).ConfigureAwait(false);
        }

        private async Task<EcoBeeAuthTokenData> RefreshAccessTokenAsync(string refreshToken)
        {
            await s_semaphore.WaitAsync();
            try
            {
                var authData = await _authCache.GetAsync().ConfigureAwait(false);
                if (authData is not null)
                    return authData;

                _logger.LogInformation("Refreshing access token.");

                var url = $"https://api.ecobee.com/token?grant_type=refresh_token&refresh_token={refreshToken}&client_id={_apiKey}&ecobee_type=jwt";
                var responseMessage = await _httpClient.PostAsJsonAsync(url, new object()).ConfigureAwait(false);
                var tokenResponse = await responseMessage.Content.ReadFromJsonAsync<TokenResponse>().ConfigureAwait(false)
                    ?? throw new EcoBeeClientAuthException("Unexpected root literal null response when refreshing the access token.");
                return await CacheAuthToken(tokenResponse).ConfigureAwait(false);
            }
            finally
            {
                s_semaphore.Release();
            }
        }

        private async Task<EcoBeeAuthTokenData> CacheAuthToken(TokenResponse tokenResponse)
        {
            var authData = new EcoBeeAuthTokenData
            {
                TokenType = tokenResponse.TokenType,
                AccessToken = tokenResponse.AccessToken
            };
            await _authCache.SetAsync(authData, TimeSpan.FromSeconds(tokenResponse.ExpiresIn)).ConfigureAwait(false);

            return authData;
        }
    }
}
