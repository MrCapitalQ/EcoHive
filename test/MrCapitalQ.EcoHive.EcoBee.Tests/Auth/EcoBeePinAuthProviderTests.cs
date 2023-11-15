using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
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

        private readonly SubstituteHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;
        private readonly FakeTimeProvider _timeProvider;
        private readonly IEcoBeeAuthCache _authCache;
        private readonly IEcoBeeRefreshTokenStore _refreshTokenStore;
        private readonly ILogger<EcoBeePinAuthProvider> _logger;

        private readonly EcoBeePinAuthProvider _ecoBeePinAuthProvider;

        public EcoBeePinAuthProviderTests()
        {
            _httpMessageHandler = HttpSubstitute.ForHandler();
            _httpClient = new(_httpMessageHandler);
            _timeProvider = Substitute.ForPartsOf<FakeTimeProvider>();
            _timeProvider.SetUtcNow(_now);
            _authCache = Substitute.For<IEcoBeeAuthCache>();
            _refreshTokenStore = Substitute.For<IEcoBeeRefreshTokenStore>();
            _logger = Substitute.For<ILogger<EcoBeePinAuthProvider>>();

            _ecoBeePinAuthProvider = new EcoBeePinAuthProvider(_httpClient,
                _timeProvider,
                _authCache,
                _refreshTokenStore,
                ApiKey,
                _logger);
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
            _httpMessageHandler.SendSubstitute(HttpArg.IsRequest(HttpMethod.Get, requestUri), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(responseBody))
                });
            var expected = new PinData
            {
                Pin = responseBody.ecobeePin,
                AuthCode = responseBody.code,
                Scope = responseBody.scope,
                Expiration = _now.AddMinutes(responseBody.expires_in)
            };

            var actual = await _ecoBeePinAuthProvider.GetPinAsync(Scope);

            Assert.Equal(expected, actual);
            _httpMessageHandler.Received(1)
                .SendSubstitute(HttpArg.IsRequest(HttpMethod.Get, requestUri), Arg.Any<CancellationToken>());
            _timeProvider.Received(1).GetUtcNow();
        }

        [Fact]
        public async Task GetPinAsync_RootLiteralNullJsonResponse_ThrowsException()
        {
            var requestUri = new Uri($"https://api.ecobee.com/authorize?response_type=ecobeePin&client_id={ApiKey}&scope={Scope}");
            _httpMessageHandler.SendSubstitute(HttpArg.IsRequest(HttpMethod.Get, requestUri), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("null")
                });

            var ex = await Assert.ThrowsAsync<EcoBeeClientAuthException>(() => _ecoBeePinAuthProvider.GetPinAsync(Scope));

            Assert.Equal("Unexpected root literal null response when requesting a pin.", ex.Message);
            _httpMessageHandler.Received(1)
                .SendSubstitute(HttpArg.IsRequest(HttpMethod.Get, requestUri), Arg.Any<CancellationToken>());
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
            _httpMessageHandler.SendSubstitute(HttpArg.IsRequest(HttpMethod.Post, requestUri), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(tokenResponseBody))
                });

            var result = await _ecoBeePinAuthProvider.AuthenticateAsync(authCode);

            Assert.True(result);
            _httpMessageHandler.Received(1)
                .SendSubstitute(HttpArg.IsRequest(HttpMethod.Post, requestUri), Arg.Any<CancellationToken>());
            await _authCache.Received(1).SetAsync(Arg.Any<EcoBeeAuthTokenData>(), Arg.Any<TimeSpan>());
            await _refreshTokenStore.Received(1).SetAsync(tokenResponseBody.refresh_token);
        }

        [Fact]
        public async Task AuthenticateAsync_Fail_ReturnsFalse()
        {
            var authCode = "fake_code";
            var requestUri = new Uri($"https://api.ecobee.com/token?grant_type=ecobeePin&code={authCode}&client_id={ApiKey}&ecobee_type=jwt");
            var tokenResponseBody = new { };
            _httpMessageHandler.SendSubstitute(HttpArg.IsRequest(HttpMethod.Post, requestUri), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(tokenResponseBody))
                });

            var result = await _ecoBeePinAuthProvider.AuthenticateAsync(authCode);

            Assert.False(result);
            _httpMessageHandler.Received(1)
                .SendSubstitute(HttpArg.IsRequest(HttpMethod.Post, requestUri), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task AuthenticateAsync_RootLiteralNullJsonResponse_ThrowsException()
        {
            var authCode = "fake_code";
            var requestUri = new Uri($"https://api.ecobee.com/token?grant_type=ecobeePin&code={authCode}&client_id={ApiKey}&ecobee_type=jwt");
            _httpMessageHandler.SendSubstitute(HttpArg.IsRequest(HttpMethod.Post, requestUri), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("null")
                });

            var ex = await Assert.ThrowsAsync<EcoBeeClientAuthException>(() => _ecoBeePinAuthProvider.AuthenticateAsync(authCode));

            Assert.Equal("Unexpected root literal null response when requesting an access token.", ex.Message);
            _httpMessageHandler.Received(1)
                .SendSubstitute(HttpArg.IsRequest(HttpMethod.Post, requestUri), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetAuthHeaderAsync_TokenExpired_RefreshesAccessToken()
        {
            var refreshToken = "fake_refresh_token";
            _refreshTokenStore.GetAsync().Returns(refreshToken);
            var tokenResponseBody = new
            {
                token_type = "bearer",
                access_token = "fake_access_token",
                refresh_Token = refreshToken,
                expires_in = 9
            };
            var requestUri = new Uri($"https://api.ecobee.com/token?grant_type=refresh_token&refresh_token={refreshToken}&client_id={ApiKey}&ecobee_type=jwt");
            _httpMessageHandler.SendSubstitute(HttpArg.IsRequest(HttpMethod.Post, requestUri), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(tokenResponseBody))
                });

            var result = await _ecoBeePinAuthProvider.GetAuthHeaderAsync(CancellationToken.None);

            Assert.Equal(new AuthenticationHeaderValue(tokenResponseBody.token_type, tokenResponseBody.access_token), result);
            _httpMessageHandler.Received(1)
                .SendSubstitute(HttpArg.IsRequest(HttpMethod.Post, requestUri), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetAuthHeaderAsync_TokenNoLongerExpiredAfterAcquiringSemaphore_ReturnsExistingAccessToken()
        {
            var authData = new EcoBeeAuthTokenData
            {
                TokenType = "bearer",
                AccessToken = "fake_access_token"
            };
            _authCache.GetAsync().Returns(null, authData);
            var refreshToken = "fake_refresh_token";
            _refreshTokenStore.GetAsync().Returns(refreshToken);

            var result = await _ecoBeePinAuthProvider.GetAuthHeaderAsync(CancellationToken.None);

            Assert.Equal(new AuthenticationHeaderValue(authData.TokenType, authData.AccessToken), result);
            _httpMessageHandler.DidNotReceiveWithAnyArgs()
                .SendSubstitute(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetAuthHeaderAsync_TokenNotExpired_ReturnsExistingAccessToken()
        {
            var authData = new EcoBeeAuthTokenData
            {
                TokenType = "bearer",
                AccessToken = "fake_access_token"
            };
            _authCache.GetAsync().Returns(authData);
            var refreshToken = "fake_refresh_token";
            _refreshTokenStore.GetAsync().Returns(refreshToken);

            var result = await _ecoBeePinAuthProvider.GetAuthHeaderAsync(CancellationToken.None);

            Assert.Equal(new AuthenticationHeaderValue(authData.TokenType, authData.AccessToken), result);
            _httpMessageHandler.DidNotReceiveWithAnyArgs()
                .SendSubstitute(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetAuthHeaderAsync_NoAuthData_ReturnsNull()
        {
            _refreshTokenStore.GetAsync().Returns((string?)null);

            var result = await _ecoBeePinAuthProvider.GetAuthHeaderAsync(CancellationToken.None);

            Assert.Null(result);
            _httpMessageHandler.DidNotReceiveWithAnyArgs()
                .SendSubstitute(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetAuthHeaderAsync_RootLiteralNullJsonResponseWhenRefreshingToken_ThrowsException()
        {
            var refreshToken = "fake_refresh_token";
            _refreshTokenStore.GetAsync().Returns(refreshToken);
            var requestUri = new Uri($"https://api.ecobee.com/token?grant_type=refresh_token&refresh_token={refreshToken}&client_id={ApiKey}&ecobee_type=jwt");
            _httpMessageHandler.SendSubstitute(HttpArg.IsRequest(HttpMethod.Post, requestUri), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("null")
                });

            var ex = await Assert.ThrowsAsync<EcoBeeClientAuthException>(() => _ecoBeePinAuthProvider.GetAuthHeaderAsync(CancellationToken.None));

            Assert.Equal("Unexpected root literal null response when refreshing the access token.", ex.Message);
            _httpMessageHandler.Received(1)
                .SendSubstitute(HttpArg.IsRequest(HttpMethod.Post, requestUri), Arg.Any<CancellationToken>());
        }

        [Fact]
        public void ClearCached_InvalidatesCache()
        {
            _ecoBeePinAuthProvider.ClearCached();

            _authCache.Received(1).Invalidate();
        }
    }
}
