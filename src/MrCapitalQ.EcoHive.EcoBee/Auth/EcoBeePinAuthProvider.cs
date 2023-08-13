using MrCapitalQ.EcoHive.EcoBee.Dtos;
using MrCapitalQ.EcoHive.EcoBee.Exceptions;
using System.Net.Http.Json;

namespace MrCapitalQ.EcoHive.EcoBee.Auth
{
    public class EcoBeePinAuthProvider : IEcoBeePinAuthProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IEcoBeeAuthCache _cache;
        private readonly string _apiKey;
        private EcoBeeAuthTokenData? _authData;

        public EcoBeePinAuthProvider(IEcoBeeAuthCache cache, string apiKey)
            : this(new HttpClient(), cache, apiKey)
        { }

        public EcoBeePinAuthProvider(HttpClient httpClient, IEcoBeeAuthCache cache, string apiKey)
        {
            _httpClient = httpClient;
            _cache = cache;
            _apiKey = apiKey;
        }

        public async Task<bool> IsAuthenticated()
        {
            await EnsureCacheRestoredAsync();
            return !string.IsNullOrEmpty(_authData?.RefreshToken);
        }

        public async Task<PinData> GetPinAsync(string scope)
        {
            var url = $"https://api.ecobee.com/authorize?response_type=ecobeePin&client_id={_apiKey}&scope={scope}";
            var pinResponse = await _httpClient.GetFromJsonAsync<PinResponse>(url)
                ?? throw new EcoBeeClientAuthException("Unexpected root literal null response when requesting a pin.");

            return new PinData
            {
                Pin = pinResponse.EcobeePin,
                AuthCode = pinResponse.Code,
                Scope = pinResponse.Scope,
                Expiration = DateTimeOffset.UtcNow.AddSeconds(pinResponse.ExpiresIn)
            };
        }

        public async Task<bool> AuthenticateAsync(string authCode)
        {
            await RequestAccessTokenAsync(authCode);
            return !string.IsNullOrEmpty(_authData?.RefreshToken);
        }

        public async Task<string> GetAccessTokenAsync()
        {
            await EnsureCacheRestoredAsync();

            if (!string.IsNullOrEmpty(_authData?.RefreshToken) && _authData.Expiration < DateTimeOffset.UtcNow)
                await RefreshAccessTokenAsync(_authData.RefreshToken);

            return _authData?.AccessToken ?? string.Empty;
        }

        private async Task EnsureCacheRestoredAsync()
        {
            _authData = await _cache.GetAysnc();
        }

        private async Task RequestAccessTokenAsync(string authCode)
        {
            var url = $"https://api.ecobee.com/token?grant_type=ecobeePin&code={authCode}&client_id={_apiKey}&ecobee_type=jwt";
            var responseMessage = await _httpClient.PostAsJsonAsync(url, new object());
            var tokenResponse = await responseMessage.Content.ReadFromJsonAsync<TokenResponse>()
                ?? throw new EcoBeeClientAuthException("Unexpected root literal null response when requesting an access token.");
            await SetAuthData(tokenResponse);
        }

        private async Task RefreshAccessTokenAsync(string refreshToken)
        {
            var url = $"https://api.ecobee.com/token?grant_type=refresh_token&refresh_token={refreshToken}&client_id={_apiKey}&ecobee_type=jwt";
            var responseMessage = await _httpClient.PostAsJsonAsync(url, new object());
            var tokenResponse = await responseMessage.Content.ReadFromJsonAsync<TokenResponse>()
                ?? throw new EcoBeeClientAuthException("Unexpected root literal null response when refreshing the access token.");
            await SetAuthData(tokenResponse);
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
                Expiration = DateTimeOffset.UtcNow.AddMinutes(tokenResponse.ExpiresIn)

            };
            await _cache.SetAysnc(_authData);
        }
    }
}
