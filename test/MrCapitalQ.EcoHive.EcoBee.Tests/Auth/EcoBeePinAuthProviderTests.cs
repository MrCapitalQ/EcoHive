using Microsoft.Extensions.Logging;
using Moq;
using MrCapitalQ.EcoHive.EcoBee.Auth;
using MrCapitalQ.EcoHive.EcoBee.Exceptions;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MrCapitalQ.EcoHive.EcoBee.Tests.Auth
{
    public class EcoBeePinAuthProviderTests
    {
        private const string ApiKey = "fake_api_key";
        private const string Scope = "fake_scope";

        private readonly Mock<HttpMessageHandler> _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;
        private readonly Mock<IDateTimeProvider> _dateTimeProvider;
        private readonly Mock<IEcoBeeAuthCache> _authCache;
        private readonly Mock<IEcoBeeRefreshTokenStore> _refreshTokenStore;
        private readonly Mock<ILogger<EcoBeePinAuthProvider>> _logger;

        private readonly EcoBeePinAuthProvider _ecoBeePinAuthProvider;

        public EcoBeePinAuthProviderTests()
        {
            _httpMessageHandler = new();
            _httpClient = new(_httpMessageHandler.Object);
            _dateTimeProvider = new();
            _dateTimeProvider.Setup(x => x.UtcNow()).Returns(_now);
            _authCache = new();
            _refreshTokenStore = new();
            _logger = new();

            _ecoBeePinAuthProvider = new EcoBeePinAuthProvider(_httpClient,
                _dateTimeProvider.Object,
                _authCache.Object,
                _refreshTokenStore.Object,
                ApiKey,
                _logger.Object);
        }

        [Fact]
        public async Task GetPinAsync_OkResponse_ReturnsPin()
        {
            var requestUri = new Uri($"https://api.ecobee.com/authorize?response_type=ecobeePin&client_id={ApiKey}&scope={Scope}");
            var responseBody = new
            {
                ecobeePin = "1234-56789",
                code = "fake_code",
                scope = "fake_scope",
                expires_in = 9
            };
            _httpMessageHandler.SetupSend(HttpMethod.Get, requestUri).ReturnsResponse(HttpStatusCode.OK, JsonSerializer.Serialize(responseBody));
            var expected = new PinData
            {
                Pin = responseBody.ecobeePin,
                AuthCode = responseBody.code,
                Scope = responseBody.scope,
                Expiration = _now.AddMinutes(responseBody.expires_in)
            };

            var actual = await _ecoBeePinAuthProvider.GetPinAsync(Scope);

            Assert.Equal(expected, actual);
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
            _authCache.Verify(c => c.SetAsync(It.IsAny<EcoBeeAuthTokenData>(), It.IsAny<TimeSpan>()), Times.Once);
            _refreshTokenStore.Verify(s => s.SetAsync(tokenResponseBody.refresh_token), Times.Once);
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
        public async Task GetAuthHeaderAsync_TokenExpired_RefreshesAccessToken()
        {
            var refreshToken = "fake_refresh_token";
            _refreshTokenStore.Setup(x => x.GetAsync()).ReturnsAsync(refreshToken);
            var tokenResponseBody = new
            {
                token_type = "bearer",
                access_token = "fake_access_token",
                refresh_Token = refreshToken,
                expires_in = 9
            };
            var requestUri = new Uri($"https://api.ecobee.com/token?grant_type=refresh_token&refresh_token={refreshToken}&client_id={ApiKey}&ecobee_type=jwt");
            _httpMessageHandler.SetupSend(HttpMethod.Post, requestUri).ReturnsResponse(HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponseBody));

            var result = await _ecoBeePinAuthProvider.GetAuthHeaderAsync(CancellationToken.None);

            Assert.Equal(new AuthenticationHeaderValue(tokenResponseBody.token_type, tokenResponseBody.access_token), result);
            _httpMessageHandler.VerifySend(HttpMethod.Post, requestUri, Times.Once);
        }

        [Fact]
        public async Task GetAuthHeaderAsync_TokenNoLongerExpiredAfterAcquiringSemaphore_ReturnsExistingAccessToken()
        {
            var authData = new EcoBeeAuthTokenData
            {
                TokenType = "bearer",
                AccessToken = "fake_access_token"
            };
            var isAfterSemaphore = false;
            _authCache.Setup(x => x.GetAsync())
                .ReturnsAsync(() => isAfterSemaphore ? authData : null)
                .Callback(() => isAfterSemaphore = true);
            var refreshToken = "fake_refresh_token";
            _refreshTokenStore.Setup(x => x.GetAsync()).ReturnsAsync(refreshToken);

            var result = await _ecoBeePinAuthProvider.GetAuthHeaderAsync(CancellationToken.None);

            Assert.Equal(new AuthenticationHeaderValue(authData.TokenType, authData.AccessToken), result);
            _httpMessageHandler.VerifySend(HttpMethod.Post, It.IsAny<Uri>(), Times.Never);
        }

        [Fact]
        public async Task GetAuthHeaderAsync_TokenNotExpired_ReturnsExistingAccessToken()
        {
            var authData = new EcoBeeAuthTokenData
            {
                TokenType = "bearer",
                AccessToken = "fake_access_token"
            };
            _authCache.Setup(x => x.GetAsync()).ReturnsAsync(authData);
            var refreshToken = "fake_refresh_token";
            _refreshTokenStore.Setup(x => x.GetAsync()).ReturnsAsync(refreshToken);
            var tokenResponseBody = new
            {
                token_type = "bearer",
                access_token = "fake_access_token",
                refresh_Token = refreshToken,
                expires_in = 9
            };

            var result = await _ecoBeePinAuthProvider.GetAuthHeaderAsync(CancellationToken.None);

            Assert.Equal(new AuthenticationHeaderValue(authData.TokenType, authData.AccessToken), result);
            _httpMessageHandler.VerifySend(HttpMethod.Post, It.IsAny<Uri>(), Times.Never);
        }

        [Fact]
        public async Task GetAuthHeaderAsync_NoAuthData_ReturnsEmptyString()
        {
            var result = await _ecoBeePinAuthProvider.GetAuthHeaderAsync(CancellationToken.None);

            Assert.Null(result);
            _httpMessageHandler.VerifySend(HttpMethod.Post, It.IsAny<Uri>(), Times.Never);
        }

        [Fact]
        public async Task GetAuthHeaderAsync_RootLiteralNullJsonResponseWhenRefreshingToken_ThrowsException()
        {
            var refreshToken = "fake_refresh_token";
            _refreshTokenStore.Setup(x => x.GetAsync()).ReturnsAsync(refreshToken);
            var requestUri = new Uri($"https://api.ecobee.com/token?grant_type=refresh_token&refresh_token={refreshToken}&client_id={ApiKey}&ecobee_type=jwt");
            _httpMessageHandler.SetupSend(HttpMethod.Post, requestUri).ReturnsResponse(HttpStatusCode.OK, "null");

            var ex = await Assert.ThrowsAsync<EcoBeeClientAuthException>(() => _ecoBeePinAuthProvider.GetAuthHeaderAsync(CancellationToken.None));

            Assert.Equal("Unexpected root literal null response when refreshing the access token.", ex.Message);
            _httpMessageHandler.VerifySend(HttpMethod.Post,
                requestUri,
                Times.Once);
        }
    }
}
