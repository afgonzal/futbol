using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Futbol.Seasons.DataRepository.DataEntities;
using Microsoft.Extensions.Configuration;

namespace Futbol.Seasons.DataRepository.Repositories
{
    public interface ITeamStatsRepository : IRepository<TeamSeasonStats>
    {
        Task<List<TeamSeasonStats>> GetSeasonTeamsStatsAsync(short year, byte season);
    }
    public class TeamStatsRepository : Repository<TeamSeasonStats>, ITeamStatsRepository
    {
        private const string TeamsTableName = "Teams";
        public TeamStatsRepository(IConfiguration config) : base(config, TeamsTableName)
        {
        }

        public Task<List<TeamSeasonStats>> GetSeasonTeamsStatsAsync(short year, byte season)
        {
            var query = Context.QueryAsync<TeamSeasonStats>(year, QueryOperator.BeginsWith, new string[] { $"SeasonStats#{season}" });
            return query.GetRemainingAsync();
        }
    }
}
