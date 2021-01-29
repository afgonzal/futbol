using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Futbol.Seasons.DataRepository.DataEntities;
using Microsoft.Extensions.Configuration;

namespace Futbol.Seasons.DataRepository.Repositories
{
    public interface ITeamRepository : IRepository<TeamProfile>
    {
        Task<List<TeamProfile>> GetYearTeamsAsync(short year);
        Task DeleteTeamsAsync(IEnumerable<TeamProfile> teams);
    }
    public class TeamRepository : Repository<TeamProfile>, ITeamRepository
    {
        private const string TeamsTableName = "Teams";
        public TeamRepository(IConfiguration config) : base(config, TeamsTableName)
        {
        }


        public Task<List<TeamProfile>> GetYearTeamsAsync(short year)
        {

            var result = _context.QueryAsync<TeamProfile>(year, QueryOperator.BeginsWith, new string[] {"Profile#"});
            return result.GetRemainingAsync();
        }

        public Task DeleteTeamsAsync(IEnumerable<TeamProfile> teams)
        {
            var batch = _context.CreateBatchWrite<TeamProfile>();
            batch.AddDeleteItems(teams);
            return batch.ExecuteAsync();
        }
    }
}
