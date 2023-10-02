using Microsoft.EntityFrameworkCore;

namespace MrCapitalQ.EcoHive.EcoBee.AspNetCore
{
    public class EcoBeeContext : DbContext
    {
        public EcoBeeContext(DbContextOptions<EcoBeeContext> options)
            : base(options)
        { }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
