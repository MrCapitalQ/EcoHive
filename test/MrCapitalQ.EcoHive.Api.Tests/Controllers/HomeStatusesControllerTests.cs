using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MrCapitalQ.EcoHive.Api.Controllers;
using MrCapitalQ.EcoHive.Api.Models;
using MrCapitalQ.EcoHive.EcoBee;
using MrCapitalQ.EcoHive.EcoBee.Functions;

namespace MrCapitalQ.EcoHive.Api.Tests.Controllers
{
    public class HomeStatusesControllerTests
    {
        private readonly Mock<IEcoBeeThermostatClient> _ecoBeeThermostatClient;
        private readonly Mock<ILogger<HomeStatusesController>> _logger;

        private readonly HomeStatusesController _controller;

        public HomeStatusesControllerTests()
        {
            _ecoBeeThermostatClient = new();
            _logger = new();

            _controller = new HomeStatusesController(_ecoBeeThermostatClient.Object, _logger.Object);
        }

        [Fact]
        public async Task GetPinAsync_Home_CallsAuthProvider()
        {
            var result = await _controller.UpdateHomeStatusAsync(OccupancyStatus.Home);

            Assert.IsType<NoContentResult>(result);
            _ecoBeeThermostatClient.Verify(c => c.RequestUpdateAsync(It.IsAny<IThermostatFunction>()), Times.Once);
        }

        [Fact]
        public async Task GetPinAsync_Away_CallsAuthProvider()
        {
            var result = await _controller.UpdateHomeStatusAsync(OccupancyStatus.Away);

            Assert.IsType<NoContentResult>(result);
            _ecoBeeThermostatClient.Verify(c => c.RequestUpdateAsync(It.IsAny<IThermostatFunction>()), Times.Once);
        }

        [Fact]
        public async Task GetPinAsync_Unknown_CallsAuthProvider()
        {
            var result = await _controller.UpdateHomeStatusAsync((OccupancyStatus)100);

            Assert.IsType<NoContentResult>(result);
            _ecoBeeThermostatClient.Verify(c => c.RequestUpdateAsync(It.IsAny<IThermostatFunction>()), Times.Never);
        }
    }
}
