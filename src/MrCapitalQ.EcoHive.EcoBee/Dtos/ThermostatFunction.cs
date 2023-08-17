using MrCapitalQ.EcoHive.EcoBee.Functions;
using System.Text.Json.Serialization;

namespace MrCapitalQ.EcoHive.EcoBee.Dtos
{
    internal class ThermostatFunction : IThermostatFunction
    {
        [JsonPropertyName("type")]
        public required string Type { get; init; }

        [JsonPropertyName("params")]
        public object? Params { get; init; }
    }
}
