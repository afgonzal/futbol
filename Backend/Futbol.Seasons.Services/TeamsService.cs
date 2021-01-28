using System;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.BusinessEntities;
using Futbol.Seasons.DataRepository.Repositories;


namespace Futbol.Seasons.Services
{
    public interface ITeamsService
    {
        public Task AddTeamAsync(Team newTeam);
    }

    public class TeamsService : ITeamsService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IMapper _mapper;

        public TeamsService(ITeamRepository teamRepository, IMapper mapper)
        {
            _teamRepository = teamRepository;
            _mapper = mapper;
        }
        public async Task AddTeamAsync(Team newTeam)
        {
            await _teamRepository.AddAsync(_mapper.Map<DataRepository.DataEntities.Team>(newTeam));
        }
    }
}
