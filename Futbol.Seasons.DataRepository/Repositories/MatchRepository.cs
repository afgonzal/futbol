using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Futbol.Seasons.DataRepository.DataEntities;
using Microsoft.Extensions.Configuration;

namespace Futbol.Seasons.DataRepository.Repositories
{
    public interface IMatchRepository : IRepository<Match>
    {
        Task<List<Match>> GetMatchesAsync(short year, byte season, byte round);
        Task DeleteMatchesAsync(IEnumerable<Match> matches);
        Task<List<Match>> GetTeamSeasonMatches(short year, byte season, int teamId);

        Task<List<Match>> GetTeamSeasonHomeMatches(short year, byte season, int teamId);
        Task<List<Match>> GetTeamSeasonAwayMatches(short year, byte season, int teamId);
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

        public Task<List<Match>> GetTeamSeasonHomeMatches(short year, byte season, int teamId)
        {
            var homeGamesQuery = new QueryOperationConfig
            {
                IndexName = "HomeTeamMatches",
                Filter = new QueryFilter("HomeTeamIdYearSeason", QueryOperator.Equal, $"{teamId}#{year}#{season}")
            };
            return Context.FromQueryAsync<Match>(homeGamesQuery).GetRemainingAsync();
        }

        public Task<List<Match>> GetTeamSeasonAwayMatches(short year, byte season, int teamId)
        {
            var awayGamesQuery = new QueryOperationConfig
            {
                IndexName = "AwayTeamMatches",
                Filter = new QueryFilter("AwayTeamIdYearSeason", QueryOperator.Equal, $"{teamId}#{year}#{season}")
            };
            return Context.FromQueryAsync<Match>(awayGamesQuery).GetRemainingAsync();
        }

        public async Task<List<Match>> GetTeamSeasonMatches(short year, byte season, int teamId)
        {
            var games = await GetTeamSeasonHomeMatches(year, season, teamId);
            games.AddRange(await GetTeamSeasonAwayMatches(year, season, teamId));
            
            return games;
        }
    }
}
