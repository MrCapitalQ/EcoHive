using Microsoft.Extensions.Logging;
using MrCapitalQ.EcoHive.EcoBee.Dtos;
using MrCapitalQ.EcoHive.EcoBee.Exceptions;
using System.Net.Http.Json;

namespace MrCapitalQ.EcoHive.EcoBee.Auth
{
    public class EcoBeePinAuthProvider : IEcoBeePinAuthProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEcoBeeAuthCache _cache;
        private readonly string _apiKey;
        private readonly ILogger<EcoBeePinAuthProvider> _logger;

        private EcoBeeAuthTokenData? _authData;

        public EcoBeePinAuthProvider(IDateTimeProvider dateTimeProvider,
            IEcoBeeAuthCache cache,
            string apiKey,
            ILogger<EcoBeePinAuthProvider> logger)
            : this(new HttpClient(), dateTimeProvider, cache, apiKey, logger)
        { }

        public EcoBeePinAuthProvider(HttpClient httpClient,
            IDateTimeProvider dateTimeProvider,
            IEcoBeeAuthCache cache,
            string apiKey,
            ILogger<EcoBeePinAuthProvider> logger)
        {
            _httpClient = httpClient;
            _dateTimeProvider = dateTimeProvider;
            _cache = cache;
            _apiKey = apiKey;
            _logger = logger;
        }

        public async Task<bool> IsAuthenticated()
        {
            await EnsureCacheRestoredAsync().ConfigureAwait(false);
            return !string.IsNullOrEmpty(_authData?.RefreshToken);
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
            await RequestAccessTokenAsync(authCode).ConfigureAwait(false);
            return !string.IsNullOrEmpty(_authData?.RefreshToken);
        }

        public async Task<string> GetAccessTokenAsync()
        {
            await EnsureCacheRestoredAsync().ConfigureAwait(false);

            if (!string.IsNullOrEmpty(_authData?.RefreshToken) && _authData.Expiration < _dateTimeProvider.UtcNow())
            {
                _logger.LogInformation("Refreshing access token. Token expired on {expiration}.", _authData.Expiration);
                await RefreshAccessTokenAsync(_authData.RefreshToken).ConfigureAwait(false);
            }

            return _authData?.AccessToken ?? string.Empty;
        }

        private async Task EnsureCacheRestoredAsync()
        {
            _authData = await _cache.GetAysnc().ConfigureAwait(false);
        }

        private async Task RequestAccessTokenAsync(string authCode)
        {
            var url = $"https://api.ecobee.com/token?grant_type=ecobeePin&code={authCode}&client_id={_apiKey}&ecobee_type=jwt";
            var responseMessage = await _httpClient.PostAsJsonAsync(url, new object()).ConfigureAwait(false);
            var tokenResponse = await responseMessage.Content.ReadFromJsonAsync<TokenResponse>().ConfigureAwait(false)
                ?? throw new EcoBeeClientAuthException("Unexpected root literal null response when requesting an access token.");
            await SetAuthData(tokenResponse).ConfigureAwait(false);
        }

        private async Task RefreshAccessTokenAsync(string refreshToken)
        {
            var url = $"https://api.ecobee.com/token?grant_type=refresh_token&refresh_token={refreshToken}&client_id={_apiKey}&ecobee_type=jwt";
            var responseMessage = await _httpClient.PostAsJsonAsync(url, new object()).ConfigureAwait(false);
            var tokenResponse = await responseMessage.Content.ReadFromJsonAsync<TokenResponse>().ConfigureAwait(false)
                ?? throw new EcoBeeClientAuthException("Unexpected root literal null response when refreshing the access token.");
            await SetAuthData(tokenResponse).ConfigureAwait(false);
        }

        private async Task SetAuthData(TokenResponse tokenResponse)
        {
            if (string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                _authData = null;
                return;
            }

            _authData = new EcoBeeAuthTokenData
            {
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                Expiration = _dateTimeProvider.UtcNow().AddSeconds(tokenResponse.ExpiresIn)
            };
            await _cache.SetAysnc(_authData).ConfigureAwait(false);
        }
    }
}
