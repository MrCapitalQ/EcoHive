namespace MrCapitalQ.EcoHive.EcoBee.Auth
{
    public interface IEcoBeeAuthCache
    {
        Task<EcoBeeAuthTokenData?> GetAsync();
        Task SetAsync(EcoBeeAuthTokenData value, TimeSpan expirationFromNow);
        void Invalidate();
    }
}
