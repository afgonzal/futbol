using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.BusinessEntities;
using Futbol.Seasons.DataRepository.Repositories;

namespace Futbol.Seasons.Services
{
    /// <summary>
    /// Handle Partners, Users, Players,  etc
    /// </summary>
    public interface IPartnerService
    {
        Task<User> GetUserByEmailAsync(string email);
    }
    public class PartnersService : IPartnerService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public PartnersService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetUserAsync(email);
            return  _mapper.Map<User>(user);
        }
    }
}
