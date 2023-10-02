using Microsoft.Extensions.Caching.Memory;
using MrCapitalQ.EcoHive.EcoBee.Auth;

namespace MrCapitalQ.EcoHive.EcoBee.AspNetCore
{
    public class EcoBeeAuthCache : IEcoBeeAuthCache
    {
        private const string AuthTokenCacheKey = "EcoBeeAuthToken";

        private readonly IMemoryCache _memoryCache;

        public EcoBeeAuthCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<EcoBeeAuthTokenData?> GetAsync()
        {
            return Task.FromResult(_memoryCache.Get<EcoBeeAuthTokenData>(AuthTokenCacheKey));
        }

        public Task SetAsync(EcoBeeAuthTokenData value, TimeSpan expirationFromNow)
        {
            _memoryCache.Set(AuthTokenCacheKey, value, expirationFromNow);
            return Task.CompletedTask;
        }
    }
}
