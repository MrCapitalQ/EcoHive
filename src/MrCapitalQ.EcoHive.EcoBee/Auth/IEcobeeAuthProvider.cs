namespace MrCapitalQ.EcoHive.EcoBee.Auth
{
    public interface IEcoBeeAuthProvider
    {
        Task<string> GetAccessTokenAsync();
    }

    public interface IEcoBeePinAuthProvider : IEcoBeeAuthProvider
    {
        Task<bool> IsAuthenticated();
        Task<PinData> GetPinAsync(string scope);
        Task<bool> AuthenticateAsync(string authCode);
    }
}