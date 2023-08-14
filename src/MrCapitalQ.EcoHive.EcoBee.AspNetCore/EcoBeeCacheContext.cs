using Microsoft.EntityFrameworkCore;

namespace MrCapitalQ.EcoHive.EcoBee.AspNetCore
{
    public class EcoBeeCacheContext : DbContext
    {
        public EcoBeeCacheContext(DbContextOptions<EcoBeeCacheContext> options)
            : base(options)
        { }

        public DbSet<AuthToken> AuthTokens { get; set; }
    }
}
