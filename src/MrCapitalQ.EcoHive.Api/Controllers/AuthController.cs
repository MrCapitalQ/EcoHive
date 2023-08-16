using Microsoft.AspNetCore.Mvc;
using MrCapitalQ.EcoHive.Api.Models;
using MrCapitalQ.EcoHive.EcoBee.Auth;

namespace MrCapitalQ.EcoHive.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IEcoBeePinAuthProvider _ecoBeePinAuthProvider;

        public AuthController(IEcoBeePinAuthProvider ecoBeePinAuthProvider)
        {
            _ecoBeePinAuthProvider = ecoBeePinAuthProvider;
        }

        [HttpGet("Pins", Name = nameof(GetPinAsync))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PinData))]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetPinAsync()
        {
            var pinData = await _ecoBeePinAuthProvider.GetPinAsync("smartWrite");
            return Ok(pinData);
        }

        [HttpPost(Name = nameof(AuthenticateAsync))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticateResult))]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> AuthenticateAsync(AuthenticateRequest request)
        {
            var result = await _ecoBeePinAuthProvider.AuthenticateAsync(request.AuthCode);
            return Ok(new AuthenticateResult { IsAuthenticated = result });
        }
    }
}
