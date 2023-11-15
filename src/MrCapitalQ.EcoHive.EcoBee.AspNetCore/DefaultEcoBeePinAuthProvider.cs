using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MrCapitalQ.EcoHive.EcoBee.Auth;
using System.Diagnostics.CodeAnalysis;

namespace MrCapitalQ.EcoHive.EcoBee.AspNetCore
{
    [ExcludeFromCodeCoverage]
    internal class DefaultEcoBeePinAuthProvider : EcoBeePinAuthProvider
    {
        public DefaultEcoBeePinAuthProvider(HttpClient httpClient,
            TimeProvider timeProvider,
            IEcoBeeAuthCache authCache,
            IEcoBeeRefreshTokenStore refreshTokenStore,
            IOptionsSnapshot<EcoBeeClientOptions> options,
            ILogger<EcoBeePinAuthProvider> logger)
            : base(httpClient,
                  timeProvider,
                  authCache,
                  refreshTokenStore,
                  options.Value.ApiKey,
                  logger)
        { }
    }
}
