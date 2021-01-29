using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Futbol.Seasons.DataRepository.DataEntities;
using Microsoft.Extensions.Configuration;

namespace Futbol.Seasons.DataRepository.Repositories
{
    public interface ITeamRepository : IRepository<Team>
    {
        Task<List<Team>> GetYearTeamsAsync(short year);
        Task DeleteTeamsAsync(IEnumerable<Team> teams);
    }
    public class TeamRepository : Repository<Team>, ITeamRepository
    {
        private const string TeamsTableName = "Teams";
        public TeamRepository(IConfiguration config) : base(config, TeamsTableName)
        {
        }


        public Task<List<Team>> GetYearTeamsAsync(short year)
        {
          
            var result = _context.QueryAsync<Team>(year);
            return result.GetRemainingAsync();
        }

        public Task DeleteTeamsAsync(IEnumerable<Team> teams)
        {
            var batch = _context.CreateBatchWrite<Team>();
            batch.AddDeleteItems(teams);
            return batch.ExecuteAsync();
        }
    }
}
