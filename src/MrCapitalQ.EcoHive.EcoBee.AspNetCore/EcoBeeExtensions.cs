using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MrCapitalQ.EcoHive.EcoBee.Auth;
using System.Diagnostics.CodeAnalysis;

namespace MrCapitalQ.EcoHive.EcoBee.AspNetCore
{
    [ExcludeFromCodeCoverage]
    public static class EcoBeeExtensions
    {
        private const string ConfigurationSectionName = "EcoBee";

        public static IServiceCollection AddEcoBee(this IServiceCollection services)
        {
            services.AddMemoryCache();

            services.AddDbContext<EcoBeeContext>((s, o) =>
            {
                var configuration = s.GetRequiredService<IConfiguration>();
                o.UseSqlite(configuration.GetConnectionString("EcobeeDb"));
            });

            services.AddOptions<EcoBeeClientOptions>()
                .BindConfiguration(ConfigurationSectionName)
                .ValidateDataAnnotations();

            services.AddHttpClient<IEcoBeePinAuthProvider, DefaultEcoBeePinAuthProvider>();
            services.AddHttpClient<IEcoBeeThermostatClient, EcoBeeThermostatClient>()
                .AddHttpMessageHandler<AuthHandler>();

            services.TryAddSingleton<TimeProvider>();
            services.TryAddTransient<IEcoBeeAuthCache, EcoBeeAuthCache>();
            services.TryAddTransient<IEcoBeeRefreshTokenStore, EcoBeeRefreshTokenStore>();
            services.TryAddTransient<IEcoBeeAuthProvider>(s => s.GetRequiredService<IEcoBeePinAuthProvider>());
            services.TryAddTransient<AuthHandler>();

            return services;
        }
    }
}
