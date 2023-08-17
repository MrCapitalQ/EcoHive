using System.Text.Json.Serialization;

namespace MrCapitalQ.EcoHive.EcoBee.Dtos
{
    internal class SelectionCriteria
    {
        [JsonPropertyName("selectionType")]
        public string SelectionType { get; init; } = "registered";

        [JsonPropertyName("selectionMatch")]
        public string SelectionMatch { get; init; } = string.Empty;
    }
}
