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

    }
    public class TeamRepository : Repository<TeamProfile>, ITeamRepository
    {
        private const string TeamsTableName = "Teams";
        public TeamRepository(IConfiguration config) : base(config, TeamsTableName)
        {
        }


        public Task<List<TeamProfile>> GetYearTeamsAsync(short year)
        {

            var query = Context.QueryAsync<TeamProfile>(year, QueryOperator.BeginsWith, new string[] {"Profile#"});
            return query.GetRemainingAsync();
        }

    }
}
