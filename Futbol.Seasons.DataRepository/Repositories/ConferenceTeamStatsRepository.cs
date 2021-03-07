using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Futbol.Seasons.DataRepository.DataEntities;
using Microsoft.Extensions.Configuration;

namespace Futbol.Seasons.DataRepository.Repositories
{
    public interface IConferenceTeamStatsRepository : IRepository<TeamConferenceStats>
    {
        Task<List<TeamConferenceStats>> GetConferenceTeamsStatsAsync(short year, byte conference);
    }
    public class ConferenceTeamStatsRepository : Repository<TeamConferenceStats>, IConferenceTeamStatsRepository
    {
        private const string TeamsTableName = "Teams";
        public ConferenceTeamStatsRepository(IConfiguration config) : base(config, TeamsTableName)
        {
        }

        public Task<List<TeamConferenceStats>> GetConferenceTeamsStatsAsync(short year, byte conference)
        {
            var query = Context.QueryAsync<TeamConferenceStats>(year, QueryOperator.BeginsWith, new string[] { $"ConferenceStats#{conference}" });
            return query.GetRemainingAsync();
        }
    }
}
