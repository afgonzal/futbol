using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Futbol.Seasons.Services
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            DataRepository.Startup.ConfigureServices(services, config);
            services.AddScoped<ITeamsService, TeamsService>();
            services.AddScoped<IMatchesService, MatchesService>();

        }
    }
}
