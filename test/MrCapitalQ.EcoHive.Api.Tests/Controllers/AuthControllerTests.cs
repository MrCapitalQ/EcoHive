using Microsoft.AspNetCore.Http;
using Moq;
using MrCapitalQ.EcoHive.Api.Controllers;
using MrCapitalQ.EcoHive.Api.Models;
using MrCapitalQ.EcoHive.EcoBee.Auth;

namespace MrCapitalQ.EcoHive.Api.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IEcoBeePinAuthProvider> _ecoBeePinAuthProvider;

        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _ecoBeePinAuthProvider = new();

            _controller = new AuthController(_ecoBeePinAuthProvider.Object);
        }

        [Fact]
        public async Task GetPinAsync_CallsAuthProvider()
        {
            var scope = "smartWrite";
            var pinData = new PinData
            {
                Pin = "1234-5678",
                AuthCode = "fake_auth_code",
                Scope = scope,
                Expiration = DateTimeOffset.UtcNow
            };
            _ecoBeePinAuthProvider.Setup(x => x.GetPinAsync(scope)).ReturnsAsync(pinData);

            var result = await _controller.GetPinAsync();

            var actual = ControllerAssert.IsObjectResult<PinData>(result, StatusCodes.Status200OK);
            Assert.Equal(pinData, actual);
            _ecoBeePinAuthProvider.Verify(x => x.GetPinAsync(scope), Times.Once);
            _ecoBeePinAuthProvider.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task AuthenticateAsync_CallsAuthProvider()
        {
            var authCode = "fake_authcode";
            var request = new AuthenticateRequest { AuthCode = authCode };
            _ecoBeePinAuthProvider.Setup(x => x.AuthenticateAsync(authCode)).ReturnsAsync(true);
            var expected = new AuthenticateResult { IsAuthenticated = true };

            var result = await _controller.AuthenticateAsync(request);

            var actual = ControllerAssert.IsObjectResult<AuthenticateResult>(result, StatusCodes.Status200OK);
            Assert.Equal(expected, actual);
            _ecoBeePinAuthProvider.Verify(x => x.AuthenticateAsync(authCode), Times.Once);
            _ecoBeePinAuthProvider.VerifyNoOtherCalls();
        }
    }
}
