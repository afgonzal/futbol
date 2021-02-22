using System;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Futbol.Seasons.DevTest
{
    class Program
    {

        static async Task Main(string[] args)
        {
            //using IHost host = CreateHostBuilder(args).Build();

            // Application code should start here.
            //var snsClient = new AmazonSimpleNotificationServiceClient(credentials, region);

            //await host.RunAsync();

            var m = new Futbol.Seasons.DataRepository.DataEntities.Match() {YearSeasonRound = "2019#3#1"};
            var r = m.Round;

        }

        //static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args);
       

        public static IConfiguration LoadConfiguration()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env}.json", true, true)
                .AddEnvironmentVariables().AddUserSecrets<Program>();

            return builder.Build();
        }
    }
}
