using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MrCapitalQ.EcoHive.EcoBee.Auth;

namespace MrCapitalQ.EcoHive.EcoBee.AspNetCore
{
    public static class EcoBeeExtensions
    {
        public static IServiceCollection AddEcoBee(this IServiceCollection services)
        {
            services.AddDbContext<EcoBeeCacheContext>((s, o) =>
            {
                var configuration = s.GetRequiredService<IConfiguration>();
                o.UseSqlite(configuration.GetConnectionString("EcobeeCache"));
            });
            services.AddScoped<IEcoBeeAuthCache, EcoBeeAuthCache>();

            services.AddHttpClient();
            services.AddTransient<IDateTimeProvider, DateTimeProvider>();
            services.AddScoped(s =>
            {
                var configuration = s.GetRequiredService<IConfiguration>();
                var httpClientFactory = s.GetRequiredService<IHttpClientFactory>();
                return ActivatorUtilities.CreateInstance<EcoBeePinAuthProvider>(s,
                    httpClientFactory.CreateClient(),
                    configuration["EcoBee:ApiKey"] ?? string.Empty);
            });
            services.AddScoped<IEcoBeeAuthProvider>(s => s.GetRequiredService<EcoBeePinAuthProvider>());
            services.AddScoped<IEcoBeePinAuthProvider>(s => s.GetRequiredService<EcoBeePinAuthProvider>());

            return services;
        }
    }
}
