using System.Text.Json.Serialization;

namespace MrCapitalQ.EcoHive.EcoBee.Dtos
{
    internal class PinResponse
    {
        [JsonPropertyName("ecobeePin")]
        public string EcobeePin { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("interval")]
        public int Interval { get; set; }
    }

}
