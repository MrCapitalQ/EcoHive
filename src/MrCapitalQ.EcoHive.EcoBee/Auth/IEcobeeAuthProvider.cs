using System.Net.Http.Headers;

namespace MrCapitalQ.EcoHive.EcoBee.Auth
{
    public interface IEcoBeeAuthProvider
    {
        Task<AuthenticationHeaderValue?> GetAuthHeaderAsync(CancellationToken cancellationToken);
    }

    public interface IEcoBeePinAuthProvider : IEcoBeeAuthProvider
    {
        Task<PinData> GetPinAsync(string scope);
        Task<bool> AuthenticateAsync(string authCode);
    }
}