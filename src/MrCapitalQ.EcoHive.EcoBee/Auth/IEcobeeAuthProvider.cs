namespace MrCapitalQ.EcoHive.EcoBee.Auth
{
    public interface IEcoBeePinAuthProvider
    {
        Task<string> GetAccessTokenAsync();
    }
}