using System.Text.Json.Serialization;

namespace MrCapitalQ.EcoHive.EcoBee.Dtos
{
    internal class ThermostatResumeProgramFunctionParams
    {
        [JsonPropertyName("resumeAll")]
        public bool ResumeAll { get; init; }
    }
}
