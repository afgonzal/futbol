using Futbol.Seasons.DataRepository.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Futbol.Seasons.DataRepository
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<ITeamStatsRepository, TeamStatsRepository>();
            services.AddScoped<IMatchRepository, MatchRepository>();
            services.AddScoped<ISeasonConfigRepository, SeasonConfigRepository>();
            services.AddScoped<IChampionshipConfigRepository, ChampionshipConfigRepository>();

        }
    }
}
