using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MrCapitalQ.EcoHive.Api.Controllers;
using MrCapitalQ.EcoHive.Api.Models;
using MrCapitalQ.EcoHive.EcoBee;
using MrCapitalQ.EcoHive.EcoBee.Functions;

namespace MrCapitalQ.EcoHive.Api.Tests.Controllers
{
    public class HomeStatusesControllerTests
    {
        private readonly IEcoBeeThermostatClient _ecoBeeThermostatClient;
        private readonly ILogger<HomeStatusesController> _logger;

        private readonly HomeStatusesController _controller;

        public HomeStatusesControllerTests()
        {
            _ecoBeeThermostatClient = Substitute.For<IEcoBeeThermostatClient>();
            _logger = Substitute.For<ILogger<HomeStatusesController>>();

            _controller = new HomeStatusesController(_ecoBeeThermostatClient, _logger);
        }

        [Fact]
        public async Task UpdateHomeStatusAsync_Home_RequestsThermostatUpdate()
        {
            var result = await _controller.UpdateHomeStatusAsync(OccupancyStatus.Home);

            Assert.IsType<NoContentResult>(result);
            await _ecoBeeThermostatClient.Received(1).RequestUpdateAsync(Arg.Any<IThermostatFunction>());
        }

        [Fact]
        public async Task UpdateHomeStatusAsync_Away_RequestsThermostatUpdate()
        {
            var result = await _controller.UpdateHomeStatusAsync(OccupancyStatus.Away);

            Assert.IsType<NoContentResult>(result);
            await _ecoBeeThermostatClient.Received(1).RequestUpdateAsync(Arg.Any<IThermostatFunction>());
        }

        [Fact]
        public async Task UpdateHomeStatusAsync_Unknown_DoesNothing()
        {
            var result = await _controller.UpdateHomeStatusAsync((OccupancyStatus)100);

            Assert.IsType<NoContentResult>(result);
            await _ecoBeeThermostatClient.DidNotReceiveWithAnyArgs().RequestUpdateAsync();
        }
    }
}
