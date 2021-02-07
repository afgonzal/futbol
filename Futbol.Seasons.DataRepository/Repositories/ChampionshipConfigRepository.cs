using System.Linq;
using System.Threading.Tasks;
using Futbol.Seasons.DataRepository.DataEntities;
using Microsoft.Extensions.Configuration;

namespace Futbol.Seasons.DataRepository.Repositories
{
    public interface IChampionshipConfigRepository : IRepository<ChampionshipConfig>
    {
        Task<ChampionshipConfig> GetYearConfig(short year);
    }
    public class ChampionshipConfigRepository : Repository<ChampionshipConfig>, IChampionshipConfigRepository
    {
        private readonly ISeasonConfigRepository _seasonsRepository;
        private const string SeasonsTableName = "Seasons";
        public ChampionshipConfigRepository(ISeasonConfigRepository seasonsRepository, IConfiguration config) : base(config, SeasonsTableName)
        {
            _seasonsRepository = seasonsRepository;
        }

        public async Task<ChampionshipConfig> GetYearConfig(short year)
        {
            var championship = GetByKeyAsync(year, "Championship");
            var yearSeasons = _seasonsRepository.QueryById(year);
            await Task.WhenAll(new Task[] {championship, yearSeasons});

            //remove seasons from year that don't belong to championship, ie summer league etc
            championship.Result.Seasons = yearSeasons.Result.Where(s => championship.Result.SeasonsIds.Contains(s.SeasonId));
            return championship.Result;
        }
    }
}