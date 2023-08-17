using MrCapitalQ.EcoHive.EcoBee.Dtos;
using System.Text.Json.Serialization;

namespace MrCapitalQ.EcoHive.EcoBee.Functions
{
    [JsonDerivedType(typeof(ThermostatFunction))]
    public interface IThermostatFunction
    { }
}
