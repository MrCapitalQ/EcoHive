using Microsoft.AspNetCore.Http;
using MrCapitalQ.EcoHive.Api.Controllers;
using MrCapitalQ.EcoHive.Api.Models;
using MrCapitalQ.EcoHive.EcoBee.Auth;

namespace MrCapitalQ.EcoHive.Api.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly IEcoBeePinAuthProvider _ecoBeePinAuthProvider;

        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _ecoBeePinAuthProvider = Substitute.For<IEcoBeePinAuthProvider>();

            _controller = new AuthController(_ecoBeePinAuthProvider);
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
            _ecoBeePinAuthProvider.GetPinAsync(scope).Returns(pinData);

            var result = await _controller.GetPinAsync();

            var actual = ControllerAssert.IsObjectResult<PinData>(result, StatusCodes.Status200OK);
            Assert.Equal(pinData, actual);
            await _ecoBeePinAuthProvider.Received(1).GetPinAsync(scope);
        }

        [Fact]
        public async Task AuthenticateAsync_CallsAuthProvider()
        {
            var authCode = "fake_authcode";
            var request = new AuthenticateRequest { AuthCode = authCode };
            _ecoBeePinAuthProvider.AuthenticateAsync(authCode).Returns(true);
            var expected = new AuthenticateResult { IsAuthenticated = true };

            var result = await _controller.AuthenticateAsync(request);

            var actual = ControllerAssert.IsObjectResult<AuthenticateResult>(result, StatusCodes.Status200OK);
            Assert.Equal(expected, actual);
            await _ecoBeePinAuthProvider.Received(1).AuthenticateAsync(authCode);
        }
    }
}
