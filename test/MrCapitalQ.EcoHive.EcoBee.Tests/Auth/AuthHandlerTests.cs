using MrCapitalQ.EcoHive.EcoBee.Auth;
using System.Net.Http.Headers;

namespace MrCapitalQ.EcoHive.EcoBee.Tests.Auth
{
    public class AuthHandlerTests
    {
        private readonly IEcoBeeAuthProvider _authProvider;

        private readonly TestAuthHandler _handler;

        public AuthHandlerTests()
        {
            _authProvider = Substitute.For<IEcoBeeAuthProvider>();
            _handler = new(_authProvider);
        }

        [Fact]
        public async Task SendAsync_NonNullAuthHeader_SetsAuthHeader()
        {
            var request = new HttpRequestMessage();
            var expected = new AuthenticationHeaderValue("bearer", "token");
            _authProvider.GetAuthHeaderAsync(Arg.Any<CancellationToken>()).Returns(expected);

            await _handler.SendAsync(request, CancellationToken.None);

            Assert.Equal(expected, request.Headers.Authorization);
        }

        [Fact]
        public async Task SendAsync_NullAuthHeader_DoesNothing()
        {
            var request = new HttpRequestMessage();

            await _handler.SendAsync(request, CancellationToken.None);

            Assert.Null(request.Headers.Authorization);
        }

        [Fact]
        public void Send_NonNullAuthHeader_SetsAuthHeader()
        {
            var request = new HttpRequestMessage();
            var expected = new AuthenticationHeaderValue("bearer", "token");
            _authProvider.GetAuthHeaderAsync(Arg.Any<CancellationToken>()).Returns(expected);

            _handler.Send(request, CancellationToken.None);

            Assert.Equal(expected, request.Headers.Authorization);
        }

        [Fact]
        public void Send_NullAuthHeader_DoesNothing()
        {
            var request = new HttpRequestMessage();

            _handler.Send(request, CancellationToken.None);

            Assert.Null(request.Headers.Authorization);
        }

        private class TestAuthHandler : AuthHandler
        {
            public TestAuthHandler(IEcoBeeAuthProvider authProvider) : base(authProvider)
            {
                InnerHandler = Substitute.For<DelegatingHandler>();
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
