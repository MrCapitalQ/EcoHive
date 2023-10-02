using MrCapitalQ.EcoHive.EcoBee.Dtos;
using MrCapitalQ.EcoHive.EcoBee.Functions;
using System.Net.Http.Json;

namespace MrCapitalQ.EcoHive.EcoBee
{
    public class EcoBeeThermostatClient : IEcoBeeThermostatClient
    {
        private const string RequestUpdateUrl = "https://api.ecobee.com/1/thermostat?format=json";

        private readonly HttpClient _httpClient;

        public EcoBeeThermostatClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UpdateRequestResult> RequestUpdateAsync(params IThermostatFunction[] functions)
        {
            var request = new ThermostatUpdateRequest
            {
                Functions = functions
            };

            var responseMessage = await _httpClient.PostAsJsonAsync(RequestUpdateUrl, request).ConfigureAwait(false);
            responseMessage.EnsureSuccessStatusCode();

            var status = await responseMessage.Content.ReadFromJsonAsync<StatusResponse>().ConfigureAwait(false);
            return new UpdateRequestResult
            {
                IsSuccessful = status?.Code == 0,
                Message = status?.Message ?? string.Empty
            };
        }
    }
}