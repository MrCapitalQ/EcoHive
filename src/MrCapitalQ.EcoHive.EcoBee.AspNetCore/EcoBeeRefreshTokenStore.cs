using Microsoft.EntityFrameworkCore;
using MrCapitalQ.EcoHive.EcoBee.Auth;

namespace MrCapitalQ.EcoHive.EcoBee.AspNetCore
{
    public class EcoBeeRefreshTokenStore : IEcoBeeRefreshTokenStore
    {
        private readonly EcoBeeContext _dbContext;

        public EcoBeeRefreshTokenStore(EcoBeeContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string?> GetAsync()
        {
            var cached = await _dbContext.RefreshTokens
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            return cached?.Token;
        }

        public async Task SetAsync(string value)
        {
            await _dbContext.RefreshTokens.ExecuteDeleteAsync();

            _dbContext.RefreshTokens.Add(new()
            {
                Token = value
            });
            await _dbContext.SaveChangesAsync();
        }
    }
}
