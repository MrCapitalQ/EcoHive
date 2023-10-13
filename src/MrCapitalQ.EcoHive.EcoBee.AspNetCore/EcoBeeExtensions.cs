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

        public static IEcoBeeBuilder AddEcoBee(this IServiceCollection services)
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

            var authClientBuilder = services.AddHttpClient<IEcoBeePinAuthProvider, DefaultEcoBeePinAuthProvider>();
            var ecoBeeClientBuilder = services.AddHttpClient<IEcoBeeThermostatClient, EcoBeeThermostatClient>()
                .AddHttpMessageHandler<AuthHandler>();

            services.TryAddTransient<IDateTimeProvider, DateTimeProvider>();
            services.TryAddTransient<IEcoBeeAuthCache, EcoBeeAuthCache>();
            services.TryAddTransient<IEcoBeeRefreshTokenStore, EcoBeeRefreshTokenStore>();
            services.TryAddTransient<IEcoBeeAuthProvider>(s => s.GetRequiredService<IEcoBeePinAuthProvider>());
            services.TryAddTransient<AuthHandler>();

            return new EcoBeeBuilder
            {
                AuthClientBuilder = authClientBuilder,
                EcoBeeClientBuilder = ecoBeeClientBuilder,
                Services = services
            };
        }

        public static IEcoBeeBuilder ConfigureHttpClients(this IEcoBeeBuilder builder, Action<IHttpClientBuilder> configure)
            => builder.ConfigureAuthHttpClient(configure).ConfigureEcoBeeHttpClient(configure);

        public static IEcoBeeBuilder ConfigureAuthHttpClient(this IEcoBeeBuilder builder, Action<IHttpClientBuilder> configure)
        {
            configure(builder.AuthClientBuilder);
            return builder;
        }

        public static IEcoBeeBuilder ConfigureEcoBeeHttpClient(this IEcoBeeBuilder builder, Action<IHttpClientBuilder> configure)
        {
            configure(builder.AuthClientBuilder);
            return builder;
        }
    }


    public interface IEcoBeeBuilder
    {
        IHttpClientBuilder AuthClientBuilder { get; }

        IHttpClientBuilder EcoBeeClientBuilder { get; }

        /// <summary>
        /// Gets the application service collection.
        /// </summary>
        IServiceCollection Services { get; }
    }

    internal class EcoBeeBuilder : IEcoBeeBuilder
    {
        public required IHttpClientBuilder AuthClientBuilder { get; init; }

        public required IHttpClientBuilder EcoBeeClientBuilder { get; init; }

        public required IServiceCollection Services { get; init; }
    }
}
