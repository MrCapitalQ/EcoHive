using Microsoft.EntityFrameworkCore;
using MrCapitalQ.EcoHive.EcoBee.Auth;

namespace MrCapitalQ.EcoHive.EcoBee.AspNetCore
{
    public class EcoBeeAuthCache : IEcoBeeAuthCache
    {
        private readonly EcoBeeCacheContext _dbContext;

        public EcoBeeAuthCache(EcoBeeCacheContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<EcoBeeAuthTokenData> GetAysnc()
        {
            var cached = await _dbContext.AuthTokens
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (cached is null)
                return null;

            return new()
            {
                AccessToken = cached.AccessToken,
                RefreshToken = cached.RefreshToken,
                Expiration = cached.Expiration
            };
        }

        public async Task SetAysnc(EcoBeeAuthTokenData value)
        {
            await _dbContext.AuthTokens.ExecuteDeleteAsync();

            if (value is null)
                return;

            _dbContext.AuthTokens.Add(new()
            {
                AccessToken = value.AccessToken,
                RefreshToken = value.RefreshToken,
                Expiration = value.Expiration
            });
            await _dbContext.SaveChangesAsync();
        }
    }
}
