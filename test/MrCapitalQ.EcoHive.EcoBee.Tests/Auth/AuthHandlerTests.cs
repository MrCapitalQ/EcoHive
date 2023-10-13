using MrCapitalQ.EcoHive.EcoBee.Auth;
using System.Net;
using System.Net.Http.Headers;

namespace MrCapitalQ.EcoHive.EcoBee.Tests.Auth
{
    public class AuthHandlerTests
    {
        private readonly IEcoBeeAuthProvider _authProvider;
        private readonly SubstituteHandler _innerHandler;

        private readonly TestAuthHandler _handler;

        public AuthHandlerTests()
        {
            _authProvider = Substitute.For<IEcoBeeAuthProvider>();
            _innerHandler = HttpSubstitute.ForHandler();
            _innerHandler.SendSubstitute(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            _handler = new(_authProvider, _innerHandler);
        }

        [Fact]
        public async Task SendAsync_NonNullAuthHeader_SetsAuthHeader()
        {
            var request = new HttpRequestMessage();
            var expected = new AuthenticationHeaderValue("bearer", "token");
            _authProvider.GetAuthHeaderAsync(Arg.Any<CancellationToken>()).Returns(expected);

            await _handler.SendAsync(request, CancellationToken.None);

            Assert.Equal(expected, request.Headers.Authorization);
            await _authProvider.Received(1).GetAuthHeaderAsync(Arg.Any<CancellationToken>());
            _authProvider.DidNotReceiveWithAnyArgs().ClearCached();
        }

        [Fact]
        public async Task SendAsync_NullAuthHeader_DoesNothing()
        {
            var request = new HttpRequestMessage();

            await _handler.SendAsync(request, CancellationToken.None);

            Assert.Null(request.Headers.Authorization);
            await _authProvider.Received(1).GetAuthHeaderAsync(Arg.Any<CancellationToken>());
            _authProvider.DidNotReceiveWithAnyArgs().ClearCached();
        }

        [Fact]
        public async Task SendAsync_UnauthorizedResponse_ClearsAuthProviderCache()
        {
            _innerHandler.SendSubstitute(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized });

            await _handler.SendAsync(new HttpRequestMessage(), CancellationToken.None);

            await _authProvider.Received(1).GetAuthHeaderAsync(Arg.Any<CancellationToken>());
            _authProvider.Received(1).ClearCached();
        }

        [Fact]
        public async Task Send_NonNullAuthHeader_SetsAuthHeader()
        {
            var request = new HttpRequestMessage();
            var expected = new AuthenticationHeaderValue("bearer", "token");
            _authProvider.GetAuthHeaderAsync(Arg.Any<CancellationToken>()).Returns(expected);

            _handler.Send(request, CancellationToken.None);

            Assert.Equal(expected, request.Headers.Authorization);
            await _authProvider.Received(1).GetAuthHeaderAsync(Arg.Any<CancellationToken>());
            _authProvider.DidNotReceiveWithAnyArgs().ClearCached();
        }

        [Fact]
        public async Task Send_NullAuthHeader_DoesNothing()
        {
            var request = new HttpRequestMessage();

            _handler.Send(request, CancellationToken.None);

            Assert.Null(request.Headers.Authorization);
            await _authProvider.Received(1).GetAuthHeaderAsync(Arg.Any<CancellationToken>());
            _authProvider.DidNotReceiveWithAnyArgs().ClearCached();
        }

        [Fact]
        public async Task Send_UnauthorizedResponse_ClearsAuthProviderCache()
        {
            _innerHandler.SendSubstitute(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized });

            _handler.Send(new HttpRequestMessage(), CancellationToken.None);

            await _authProvider.Received(1).GetAuthHeaderAsync(Arg.Any<CancellationToken>());
            _authProvider.Received(1).ClearCached();
        }

        private class TestAuthHandler : AuthHandler
        {
            public TestAuthHandler(IEcoBeeAuthProvider authProvider, HttpMessageHandler innerHandler) : base(authProvider)
            {
                InnerHandler = innerHandler;
            }

            public new Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return base.SendAsync(request, cancellationToken);
            }

            public new HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return base.Send(request, cancellationToken);
            }
        }
    }
}
