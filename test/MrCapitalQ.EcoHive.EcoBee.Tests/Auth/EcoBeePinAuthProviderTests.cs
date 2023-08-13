using Moq;
using MrCapitalQ.EcoHive.EcoBee.Auth;
using MrCapitalQ.EcoHive.EcoBee.Exceptions;
using System.Net;
using System.Text.Json;

namespace MrCapitalQ.EcoHive.EcoBee.Tests.Auth
{
    public class EcoBeePinAuthProviderTests
    {
        private const string ApiKey = "fake_api_key";
        private const string Scope = "fake_scope";
        private readonly Mock<HttpMessageHandler> _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly Mock<IEcoBeeAuthCache> _cache;

        private readonly EcoBeePinAuthProvider _ecoBeePinAuthProvider;

        public EcoBeePinAuthProviderTests()
        {
            _httpMessageHandler = new();
            _httpClient = new(_httpMessageHandler.Object);
            _cache = new();

            _ecoBeePinAuthProvider = new EcoBeePinAuthProvider(_httpClient, _cache.Object, ApiKey);
        }

        [Fact]
        public async Task IsAuthenticated_NoRefreshTokenCached_ReturnsFalse()
        {
            var result = await _ecoBeePinAuthProvider.IsAuthenticated();

            Assert.False(result);
            _cache.Verify(x => x.GetAysnc(), Times.Once);
        }

        [Fact]
        public async Task IsAuthenticated_RefreshTokenCached_ReturnsTrue()
        {
            var authData = new EcoBeeAuthTokenData
            {
                AccessToken = "fake_access_token",
                RefreshToken = "fake_refresh_token",
                Expiration = DateTime.UtcNow
            };
            _cache.Setup(x => x.GetAysnc()).ReturnsAsync(authData);

            var result = await _ecoBeePinAuthProvider.IsAuthenticated();

            Assert.True(result);
            _cache.Verify(x => x.GetAysnc(), Times.Once);
        }

        [Fact]
        public async Task GetPinAsync_OkResponse_ReturnsPin()
        {
            var requestUri = new Uri($"https://api.ecobee.com/authorize?response_type=ecobeePin&client_id={ApiKey}&scope={Scope}");
            var responseBody = new
            {
                ecobeePin = "1234-56789",
                code = "fake_code",
                scope = "fake_scope"
            };
            _httpMessageHandler.SetupSend(HttpMethod.Get, requestUri).ReturnsResponse(HttpStatusCode.OK, JsonSerializer.Serialize(responseBody));

            var result = await _ecoBeePinAuthProvider.GetPinAsync(Scope);

            Assert.Equal(responseBody.ecobeePin, result.Pin);
            Assert.Equal(responseBody.code, result.AuthCode);
            Assert.Equal(responseBody.scope, result.Scope);
            _httpMessageHandler.VerifySend(HttpMethod.Get, requestUri, Times.Once);
        }

        [Fact]
        public async Task GetPinAsync_RootLiteralNullJsonResponse_ThrowsException()
        {
            var requestUri = new Uri($"https://api.ecobee.com/authorize?response_type=ecobeePin&client_id={ApiKey}&scope={Scope}");
            _httpMessageHandler.SetupSend(HttpMethod.Get, requestUri).ReturnsResponse(HttpStatusCode.OK, "null");

            var ex = await Assert.ThrowsAsync<EcoBeeClientAuthException>(() => _ecoBeePinAuthProvider.GetPinAsync(Scope));

            Assert.Equal("Unexpected root literal null response when requesting a pin.", ex.Message);
            _httpMessageHandler.VerifySend(HttpMethod.Get, requestUri, Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_Success_ReturnsTrue()
        {
            var authCode = "fake_code";
            var requestUri = new Uri($"https://api.ecobee.com/token?grant_type=ecobeePin&code={authCode}&client_id={ApiKey}&ecobee_type=jwt");
            var tokenResponseBody = new
            {
                access_token = "fake_access_token",
                refresh_token = "fake_refresh_token"
            };
            _httpMessageHandler.SetupSend(HttpMethod.Post, requestUri).ReturnsResponse(HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponseBody));

            var result = await _ecoBeePinAuthProvider.AuthenticateAsync(authCode);

            Assert.True(result);
            _httpMessageHandler.VerifySend(HttpMethod.Post, requestUri, Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_Fail_ReturnsFalse()
        {
            var authCode = "fake_code";
            var requestUri = new Uri($"https://api.ecobee.com/token?grant_type=ecobeePin&code={authCode}&client_id={ApiKey}&ecobee_type=jwt");
            var tokenResponseBody = new { };
            _httpMessageHandler.SetupSend(HttpMethod.Post, requestUri).ReturnsResponse(HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponseBody));

            var result = await _ecoBeePinAuthProvider.AuthenticateAsync(authCode);

            Assert.False(result);
            _httpMessageHandler.VerifySend(HttpMethod.Post, requestUri, Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_RootLiteralNullJsonResponse_ThrowsException()
        {
            var authCode = "fake_code";
            var requestUri = new Uri($"https://api.ecobee.com/token?grant_type=ecobeePin&code={authCode}&client_id={ApiKey}&ecobee_type=jwt");
            _httpMessageHandler.SetupSend(HttpMethod.Post, requestUri).ReturnsResponse(HttpStatusCode.OK, "null");

            var ex = await Assert.ThrowsAsync<EcoBeeClientAuthException>(() => _ecoBeePinAuthProvider.AuthenticateAsync(authCode));

            Assert.Equal("Unexpected root literal null response when requesting an access token.", ex.Message);
            _httpMessageHandler.VerifySend(HttpMethod.Post, requestUri, Times.Once);
        }

        [Fact]
        public async Task GetAccessTokenAsync_TokenExpired_RefreshesAccessToken()
        {
            var authData = new EcoBeeAuthTokenData
            {
                AccessToken = "fake_access_token",
                RefreshToken = "fake_refresh_token",
                Expiration = DateTimeOffset.MinValue
            };
            _cache.Setup(x => x.GetAysnc()).ReturnsAsync(authData);
            var tokenResponseBody = new
            {
                access_token = "fake_access_token",
                refresh_Token = "fake_refresh_token"
            };
            var requestUri = new Uri($"https://api.ecobee.com/token?grant_type=refresh_token&refresh_token={authData.RefreshToken}&client_id={ApiKey}&ecobee_type=jwt");
            _httpMessageHandler.SetupSend(HttpMethod.Post, requestUri).ReturnsResponse(HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponseBody));

            var result = await _ecoBeePinAuthProvider.GetAccessTokenAsync();

            Assert.Equal(tokenResponseBody.access_token, result);
            _httpMessageHandler.VerifySend(HttpMethod.Post, requestUri, Times.Once);
        }

        [Fact]
        public async Task GetAccessTokenAsync_TokenNotExpired_ReturnsExistingAccessToken()
        {
            var authData = new EcoBeeAuthTokenData
            {
                AccessToken = "fake_access_token",
                RefreshToken = "fake_refresh_token",
                Expiration = DateTimeOffset.MaxValue
            };
            _cache.Setup(x => x.GetAysnc()).ReturnsAsync(authData);
            var tokenResponseBody = new
            {
                access_token = "fake_access_token"
            };
            var requestUri = new Uri($"https://api.ecobee.com/token?grant_type=refresh_token&refresh_token={authData.RefreshToken}&client_id={ApiKey}&ecobee_type=jwt");
            _httpMessageHandler.SetupSend(HttpMethod.Post, requestUri).ReturnsResponse(HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponseBody));

            var result = await _ecoBeePinAuthProvider.GetAccessTokenAsync();

            Assert.Equal(tokenResponseBody.access_token, result);
            _httpMessageHandler.VerifySend(HttpMethod.Post, requestUri, Times.Never);
        }

        [Fact]
        public async Task GetAccessTokenAsync_NoAuthData_ReturnsEmptyString()
        {
            var result = await _ecoBeePinAuthProvider.GetAccessTokenAsync();

            Assert.Equal(string.Empty, result);
            _httpMessageHandler.VerifySend(HttpMethod.Post, It.IsAny<Uri>(), Times.Never);
        }

        [Fact]
        public async Task GetAccessTokenAsync_RootLiteralNullJsonResponseWhenRefreshingToken_ThrowsException()
        {
            var authData = new EcoBeeAuthTokenData
            {
                AccessToken = "fake_access_token",
                RefreshToken = "fake_refresh_token",
                Expiration = DateTime.UtcNow
            };
            _cache.Setup(x => x.GetAysnc()).ReturnsAsync(authData);
            var tokenResponseBody = new
            {
                access_token = "fake_access_token"
            };
            var requestUri = new Uri($"https://api.ecobee.com/token?grant_type=refresh_token&refresh_token={authData.RefreshToken}&client_id={ApiKey}&ecobee_type=jwt");
            _httpMessageHandler.SetupSend(HttpMethod.Post, requestUri).ReturnsResponse(HttpStatusCode.OK, "null");

            var ex = await Assert.ThrowsAsync<EcoBeeClientAuthException>(_ecoBeePinAuthProvider.GetAccessTokenAsync);

            Assert.Equal("Unexpected root literal null response when refreshing the access token.", ex.Message);
            _httpMessageHandler.VerifySend(HttpMethod.Post,
                requestUri,
                Times.Once);
        }
    }
}
