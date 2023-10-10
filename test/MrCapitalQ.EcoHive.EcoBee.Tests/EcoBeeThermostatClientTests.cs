using MrCapitalQ.EcoHive.EcoBee.Functions;
using System.Net;
using System.Text.Json;

namespace MrCapitalQ.EcoHive.EcoBee.Tests
{
    public class EcoBeeThermostatClientTests
    {
        private readonly Uri _updateUri = new("https://api.ecobee.com/1/thermostat?format=json");
        private readonly SubstituteHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;

        private readonly EcoBeeThermostatClient _ecoBeeThermostatClient;

        public EcoBeeThermostatClientTests()
        {
            _httpMessageHandler = Substitute.ForPartsOf<SubstituteHandler>();
            _httpClient = new HttpClient(_httpMessageHandler);

            _ecoBeeThermostatClient = new(_httpClient);
        }

        [Fact]
        public async Task RequestUpdateAsync_ResponseReceived_ReturnsSuccessResult()
        {
            var responseBody = new
            {
                code = 0,
                message = "test"
            };
            _httpMessageHandler.SendSubstitute(HttpArg.IsRequest(HttpMethod.Post, _updateUri), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(responseBody))
                });
            var expected = new UpdateRequestResult { IsSuccessful = true, Message = responseBody.message };

            var actual = await _ecoBeeThermostatClient.RequestUpdateAsync(Substitute.For<IThermostatFunction>());

            Assert.Equal(expected, actual);
            _httpMessageHandler.Received(1)
                .SendSubstitute(HttpArg.IsRequest(HttpMethod.Post, _updateUri), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task RequestUpdateAsync_RootLiteralNullJsonResponse_ReturnsSuccessResult2()
        {
            _httpMessageHandler.SendSubstitute(HttpArg.IsRequest(HttpMethod.Post, _updateUri), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("null")
                });
            var expected = new UpdateRequestResult { IsSuccessful = false };

            var actual = await _ecoBeeThermostatClient.RequestUpdateAsync(Substitute.For<IThermostatFunction>());

            Assert.Equal(expected, actual);
            _httpMessageHandler.Received(1)
                .SendSubstitute(HttpArg.IsRequest(HttpMethod.Post, _updateUri), Arg.Any<CancellationToken>());
        }
    }
}