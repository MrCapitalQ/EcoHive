using Moq;
using MrCapitalQ.EcoHive.EcoBee.Auth;
using MrCapitalQ.EcoHive.EcoBee.Functions;
using System.Net;
using System.Text.Json;

namespace MrCapitalQ.EcoHive.EcoBee.Tests
{
    public class EcoBeeThermostatClientTests
    {
        private readonly Uri _updateUri = new("https://api.ecobee.com/1/thermostat?format=json");
        private readonly Mock<HttpMessageHandler> _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly Mock<IEcoBeeAuthProvider> _authProvider;

        private readonly EcoBeeThermostatClient _ecoBeeThermostatClient;

        public EcoBeeThermostatClientTests()
        {
            _httpMessageHandler = new();
            _httpClient = new HttpClient(_httpMessageHandler.Object);
            _authProvider = new();

            _ecoBeeThermostatClient = new(_httpClient, _authProvider.Object);
        }

        [Fact]
        public async Task RequestUpdateAsync_ResponseReceived_ReturnsSuccessResult()
        {
            var responseBody = new
            {
                code = 0,
                message = "test"
            };
            _httpMessageHandler.SetupSend(HttpMethod.Post, _updateUri).ReturnsResponse(HttpStatusCode.OK, JsonSerializer.Serialize(responseBody));
            var expected = new UpdateRequestResult { IsSuccessful = true, Message = responseBody.message };

            var actual = await _ecoBeeThermostatClient.RequestUpdateAsync(new Mock<IThermostatFunction>().Object);

            Assert.Equal(expected, actual);
            _httpMessageHandler.VerifySend(HttpMethod.Post, _updateUri, Times.Once);
            _httpMessageHandler.VerifyNoOtherCalls();
            _authProvider.Verify(x => x.GetAccessTokenAsync(), Times.Once);
            _authProvider.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task RequestUpdateAsync_RootLiteralNullJsonResponse_ReturnsSuccessResult2()
        {
            _httpMessageHandler.SetupSend(HttpMethod.Post, _updateUri).ReturnsResponse(HttpStatusCode.OK, "null");
            var expected = new UpdateRequestResult { IsSuccessful = false };

            var actual = await _ecoBeeThermostatClient.RequestUpdateAsync(new Mock<IThermostatFunction>().Object);

            Assert.Equal(expected, actual);
            _httpMessageHandler.VerifySend(HttpMethod.Post, _updateUri, Times.Once); ;
            _httpMessageHandler.VerifyNoOtherCalls();
            _authProvider.Verify(x => x.GetAccessTokenAsync(), Times.Once);
            _authProvider.VerifyNoOtherCalls();
        }
    }
}