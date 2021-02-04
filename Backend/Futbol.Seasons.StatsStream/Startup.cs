using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using AutoMapper;

namespace Futbol.Seasons.StatsStream
{
    public class Startup
    {
        public static IServiceCollection ConfigureServices()
        {
            var config = LoadConfiguration();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);

            services.AddAutoMapper(typeof(Services.EntitiesMappingProfile));

            Services.Startup.ConfigureServices(services, config);

            

            services.AddTransient<ITeamsStatsAggregationService,TeamsStatsAggregationService>();


            return services;
        }

        public static string EnvironmentName => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Production;

        public static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(string.IsNullOrEmpty(EnvironmentName) ?
                    "appsettings.json" :
                    $"appsettings.{EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();
            if (EnvironmentName == Environments.Development)
                builder.AddUserSecrets<Startup>();
            return builder.Build();
        }
    }
}
