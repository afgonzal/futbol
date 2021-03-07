using System.Threading.Tasks;
using Futbol.Seasons.DataRepository.DataEntities;
using Microsoft.Extensions.Configuration;

namespace Futbol.Seasons.DataRepository.Repositories
{
    public interface ICurrentSeasonConfigRepository : IRepository<CurrentSeasonConfig>
    {
        Task<CurrentSeasonConfig> GetCurrentSeason(short year);
    }
    public class CurrentSeasonConfigRepository : Repository<CurrentSeasonConfig>, ICurrentSeasonConfigRepository
    {
        private const string SeasonsTableName = "Seasons";
        private const string CurrentSk = "Current";
        public CurrentSeasonConfigRepository(IConfiguration config) : base(config, SeasonsTableName)
        {
            
        }

        public Task<CurrentSeasonConfig> GetCurrentSeason(short year)
        {
            return GetByKeyAsync(year, CurrentSk);
        }
    }
}
