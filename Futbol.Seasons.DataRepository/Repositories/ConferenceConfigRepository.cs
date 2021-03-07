using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Futbol.Seasons.DataRepository.DataEntities;
using Microsoft.Extensions.Configuration;

namespace Futbol.Seasons.DataRepository.Repositories
{
    public interface IConferenceConfigRepository : IRepository<ConferenceConfig>
    {
        Task<ConferenceConfig> GetConferencesConfig(short year);
    }
    public class ConferenceConfigRepository : Repository<ConferenceConfig>, IConferenceConfigRepository
    {
        private readonly ISeasonConfigRepository _seasonsRepository;
        private const string SeasonsTableName = "Seasons";
        public ConferenceConfigRepository(ISeasonConfigRepository seasonsRepository, IConfiguration config) : base(config, SeasonsTableName)
        {
            _seasonsRepository = seasonsRepository;
        }

        public async Task<ConferenceConfig> GetConferencesConfig(short year)
        {
            var conferences = GetByKeyAsync(year, "ConferenceCups");
            var yearSeasons =
                _seasonsRepository.QueryByKeysAsync(year, new string[] {"Season#"}, QueryOperator.BeginsWith);
            await Task.WhenAll(new Task[] {conferences, yearSeasons});

            //remove seasons from year that don't belong to championship, ie summer league etc
            conferences.Result.Seasons = yearSeasons.Result.Where(s => conferences.Result.SeasonsIds.Contains(s.SeasonId));

            //build Conferences
            conferences.Result.Conferences = conferences.Result.ConferencesIds.Select(id =>
                new Conference {Id = id, Name = conferences.Result.ConferencesNames[id]});

            return conferences.Result;
        }
    }
}