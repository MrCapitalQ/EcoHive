namespace MrCapitalQ.EcoHive.EcoBee.Auth
{
    public interface IEcoBeeAuthCache
    {
        Task<EcoBeeAuthTokenData?> GetAysnc();
        Task SetAysnc(EcoBeeAuthTokenData value);
    }
}
