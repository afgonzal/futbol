using System.Threading.Tasks;
using Futbol.Seasons.DataRepository.DataEntities;
using Microsoft.Extensions.Configuration;

namespace Futbol.Seasons.DataRepository.Repositories
{
    public interface IPartnerRepository : IRepository<Partner>
    {
        Task<Partner> GetPartnerAsync(long partnerId);

    }
    public class PartnerRepository : Repository<Partner>, IPartnerRepository
    {
        private const string TableName = "Partners";
        public PartnerRepository(IConfiguration config) : base(config, TableName)
        {
        }

        public Task<Partner> GetPartnerAsync(long partnerId)
        {
            return base.GetByKeyAsync(partnerId, Partner.PartnerSk);
        }
    }
}
