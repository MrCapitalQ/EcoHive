using Microsoft.AspNetCore.Mvc;
using MrCapitalQ.EcoHive.Api.Models;
using MrCapitalQ.EcoHive.EcoBee;
using MrCapitalQ.EcoHive.EcoBee.Functions;
using System.ComponentModel.DataAnnotations;

namespace MrCapitalQ.EcoHive.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeStatusesController : ControllerBase
    {
        private readonly IEcoBeeThermostatClient _ecoBeeThermostatClient;
        private readonly ILogger<HomeStatusesController> _logger;

        public HomeStatusesController(IEcoBeeThermostatClient ecoBeeThermostatClient, ILogger<HomeStatusesController> logger)
        {
            _ecoBeeThermostatClient = ecoBeeThermostatClient;
            _logger = logger;
        }

        [HttpPut(Name = nameof(UpdateHomeStatusAsync))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> UpdateHomeStatusAsync([Required] OccupancyStatus occupancyStatus)
        {
            _logger.LogInformation("Setting home occupancy status to {status}.", occupancyStatus);

            UpdateRequestResult? result = null;
            switch (occupancyStatus)
            {
                case OccupancyStatus.Home:
                    _logger.LogInformation("Requesting thermostat to resume program.");
                    result = await _ecoBeeThermostatClient.RequestUpdateAsync(new ResumeProgramFunctionBuilder().Build());
                    break;
                case OccupancyStatus.Away:
                    _logger.LogInformation("Requesting thermostat to hold away climate.");
                    result = await _ecoBeeThermostatClient.RequestUpdateAsync(new SetHoldFunctionBuilder("away")
                        .HoldIndefinitely()
                        .Build());
                    break;
            }

            _logger.LogInformation("Thermostat function update request completed {completionStatus}.{message}",
                result?.IsSuccessful == false ? "unsuccessfully" : "successfully",
                !string.IsNullOrWhiteSpace(result?.Message) ? result.Message : string.Empty);

            return NoContent();
        }
    }
}
