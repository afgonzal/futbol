using System.Collections.Generic;
using System.Threading.Tasks;
using Futbol.Seasons.DataRepository.DataEntities;
using Microsoft.Extensions.Configuration;

namespace Futbol.Seasons.DataRepository.Repositories
{
    public interface IMatchRepository : IRepository<Match>
    {
        Task<List<Match>> GetMatchesAsync(short year, byte season, byte round);
        Task DeleteMatchesAsync(IEnumerable<Match> matches);
    }
    public class MatchRepository : Repository<Match>, IMatchRepository
    {
        private const string MatchesTableName = "Matches";
        public MatchRepository(IConfiguration config) : base(config, MatchesTableName)
        {
        }


        public Task<List<Match>> GetMatchesAsync(short year, byte season, byte round)
        {
            var result = Context.QueryAsync<Match>($"{year}#{season}#{round}");
            return result.GetRemainingAsync();
        }

        public Task DeleteMatchesAsync(IEnumerable<Match> matches)
        {
            var batch = Context.CreateBatchWrite<Match>();
            batch.AddDeleteItems(matches);
            return batch.ExecuteAsync();
        }
    }
}
