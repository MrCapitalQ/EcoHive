using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Testing;
using MrCapitalQ.EcoHive.Api.Controllers;
using MrCapitalQ.EcoHive.Api.Models;
using MrCapitalQ.EcoHive.EcoBee;
using MrCapitalQ.EcoHive.EcoBee.Functions;

namespace MrCapitalQ.EcoHive.Api.Tests.Controllers
{
    public class HomeStatusesControllerTests
    {
        private readonly IEcoBeeThermostatClient _ecoBeeThermostatClient;
        private readonly FakeLogger<HomeStatusesController> _logger;

        private readonly HomeStatusesController _controller;

        public HomeStatusesControllerTests()
        {
            _ecoBeeThermostatClient = Substitute.For<IEcoBeeThermostatClient>();
            _logger = new();

            _controller = new(_ecoBeeThermostatClient, _logger);
        }

        [Fact]
        public async Task UpdateHomeStatusAsync_Home_RequestsThermostatUpdate()
        {
            _ecoBeeThermostatClient.RequestUpdateAsync(Arg.Any<IThermostatFunction>()).Returns(new UpdateRequestResult { IsSuccessful = true });
            var result = await _controller.UpdateHomeStatusAsync(OccupancyStatus.Home);

            Assert.IsType<NoContentResult>(result);
            var loggerSnapshot = _logger.Collector.GetSnapshot();
            Assert.Equal(3, _logger.Collector.Count);
            Assert.Equal("Setting home occupancy status to Home.", loggerSnapshot[0].Message);
            Assert.Equal("Requesting thermostat to resume program.", loggerSnapshot[1].Message);
            Assert.Equal("Thermostat function update request completed successfully.", loggerSnapshot[2].Message);
            await _ecoBeeThermostatClient.Received(1).RequestUpdateAsync(Arg.Any<IThermostatFunction>());
        }

        [Fact]
        public async Task UpdateHomeStatusAsync_Away_RequestsThermostatUpdate()
        {
            _ecoBeeThermostatClient.RequestUpdateAsync(Arg.Any<IThermostatFunction>())
                .Returns(new UpdateRequestResult
                {
                    IsSuccessful = false,
                    Message = "Something went wrong."
                });
            var result = await _controller.UpdateHomeStatusAsync(OccupancyStatus.Away);

            Assert.IsType<NoContentResult>(result);
            var loggerSnapshot = _logger.Collector.GetSnapshot();
            Assert.Equal(3, _logger.Collector.Count);
            Assert.Equal("Setting home occupancy status to Away.", loggerSnapshot[0].Message);
            Assert.Equal("Requesting thermostat to hold away climate.", loggerSnapshot[1].Message);
            Assert.Equal("Thermostat function update request failed. Something went wrong.", loggerSnapshot[2].Message);
            await _ecoBeeThermostatClient.Received(1).RequestUpdateAsync(Arg.Any<IThermostatFunction>());
        }

        [Fact]
        public async Task UpdateHomeStatusAsync_Unknown_DoesNothing()
        {
            var result = await _controller.UpdateHomeStatusAsync((OccupancyStatus)100);

            Assert.IsType<NoContentResult>(result);
            var loggerSnapshot = _logger.Collector.GetSnapshot();
            Assert.Equal(2, _logger.Collector.Count);
            Assert.Equal("Setting home occupancy status to 100.", loggerSnapshot[0].Message);
            Assert.Equal("Unknown occupancy status 100.", loggerSnapshot[1].Message);
            await _ecoBeeThermostatClient.DidNotReceiveWithAnyArgs().RequestUpdateAsync();
        }
    }
}
