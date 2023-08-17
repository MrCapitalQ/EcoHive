using System.Text.Json.Serialization;

namespace MrCapitalQ.EcoHive.EcoBee.Dtos
{
    internal class ThermostatHoldFunctionParams
    {
        [JsonPropertyName("coolHoldTemp")]
        public int? CoolHoldTemp { get; init; }

        [JsonPropertyName("heatHoldTemp")]
        public int? HeatHoldTemp { get; init; }

        [JsonPropertyName("holdClimateRef")]
        public string? HoldClimateRef { get; init; }

        [JsonPropertyName("fanSpeed")]
        public string? FanSpeed { get; init; }

        [JsonPropertyName("startDate")]
        public string? StartDate { get; init; }

        [JsonPropertyName("startTime")]
        public string? StartTime { get; init; }

        [JsonPropertyName("endDate")]
        public string? EndDate { get; init; }

        [JsonPropertyName("endTime")]
        public string? EndTime { get; init; }

        [JsonPropertyName("holdType")]
        public string? HoldType { get; init; }

        [JsonPropertyName("holdHours")]
        public int? HoldHours { get; init; }
    }
}
