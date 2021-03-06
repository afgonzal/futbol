using System.Threading.Tasks;
using Futbol.Seasons.DataRepository.DataEntities;
using Microsoft.Extensions.Configuration;

namespace Futbol.Seasons.DataRepository.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetUserAsync(string email);

    }
    public class UserRepository : Repository<User>, IUserRepository
    {
        private const string TableName = "Partners";
        public UserRepository(IConfiguration config) : base(config, TableName)
        {
        }

        public Task<User> GetUserAsync(string email)
        {
            return base.GetByKeyAsync(email, User.UserSk);
        }
    }

    
}
