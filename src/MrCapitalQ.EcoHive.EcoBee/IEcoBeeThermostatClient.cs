using MrCapitalQ.EcoHive.EcoBee.Functions;

namespace MrCapitalQ.EcoHive.EcoBee
{
    public interface IEcoBeeThermostatClient
    {
        Task<UpdateRequestResult> RequestUpdateAsync(params IThermostatFunction[] functions);
    }
}