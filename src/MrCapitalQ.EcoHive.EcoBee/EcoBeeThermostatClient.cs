using MrCapitalQ.EcoHive.EcoBee.Auth;
using MrCapitalQ.EcoHive.EcoBee.Dtos;
using MrCapitalQ.EcoHive.EcoBee.Functions;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MrCapitalQ.EcoHive.EcoBee
{
    public class EcoBeeThermostatClient : IEcoBeeThermostatClient
    {
        private readonly HttpClient _httpClient;
        private readonly IEcoBeeAuthProvider _authProvider;

        public EcoBeeThermostatClient(HttpClient httpClient, IEcoBeeAuthProvider authProvider)
        {
            _httpClient = httpClient;
            _authProvider = authProvider;
        }

        public async Task<UpdateRequestResult> RequestUpdateAsync(params IThermostatFunction[] functions)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                await _authProvider.GetAccessTokenAsync().ConfigureAwait(false));

            var postRequestUrl = "https://api.ecobee.com/1/thermostat?format=json";
            var request = new ThermostatUpdateRequest
            {
                Functions = functions
            };

            var responseMessage = await _httpClient.PostAsJsonAsync(postRequestUrl, request).ConfigureAwait(false);
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