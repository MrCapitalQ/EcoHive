namespace MrCapitalQ.EcoHive.EcoBee.Auth
{
    public interface IEcoBeeRefreshTokenStore
    {
        Task<string?> GetAsync();
        Task SetAsync(string value);
    }
}
