using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.BusinessEntities;
using Futbol.Seasons.DataRepository.Repositories;


namespace Futbol.Seasons.Services
{
    public interface ITeamsService
    {
        Task AddTeamAsync(Team newTeam);
        Task<IEnumerable<Team>> GetYearTeamsAsync(short year);
        Task DeleteAllTeamsFromSeasonAsync(short year);

        Task BulkAddTeams(IEnumerable<Team> newTeams);
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

        public async Task<IEnumerable<Team>> GetYearTeamsAsync(short year)
        {
            var teams = await _teamRepository.GetYearTeamsAsync(year);
            return _mapper.Map<IEnumerable<Team>>(teams);
        }

        public async Task DeleteAllTeamsFromSeasonAsync(short year)
        {
            var teams = await _teamRepository.GetYearTeamsAsync(year);
            await _teamRepository.DeleteTeamsAsync(teams);
        }

        public Task BulkAddTeams(IEnumerable<Team> newTeams)
        {
            return _teamRepository.BatchAddAsync(_mapper.Map<IEnumerable<DataRepository.DataEntities.Team>>(newTeams));
        }
    }
}
