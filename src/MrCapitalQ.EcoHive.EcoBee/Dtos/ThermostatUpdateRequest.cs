using MrCapitalQ.EcoHive.EcoBee.Functions;
using System.Text.Json.Serialization;

namespace MrCapitalQ.EcoHive.EcoBee.Dtos
{
    internal class ThermostatUpdateRequest
    {
        [JsonPropertyName("selection")]
        public SelectionCriteria Selection { get; init; } = new();

        [JsonPropertyName("functions")]
        public IThermostatFunction[]? Functions { get; init; }
    }
}
