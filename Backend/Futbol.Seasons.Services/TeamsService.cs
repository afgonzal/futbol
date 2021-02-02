using System.Collections.Generic;
using System.Linq;
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
        Task BulkAddTeamsAsync(IEnumerable<Team> newTeams);
        Task<IEnumerable<TeamSeasonStats>> GetSeasonsTeamsStatsAsync(short year, byte season);

        Task<TeamSeasonStats> AddTeamStatsAsync(short year, byte season, int teamId, string teamName);
        Task BulkUpsertTeamStats(short year, byte season, IEnumerable<TeamSeasonStats> stats);
    }

    public class TeamsService : ITeamsService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly ITeamStatsRepository _statsRepository;
        private readonly IMapper _mapper;

        public TeamsService(ITeamRepository teamRepository,ITeamStatsRepository statsRepository,  IMapper mapper)
        {
            _teamRepository = teamRepository;
            _statsRepository = statsRepository;
            _mapper = mapper;
        }
        public async Task AddTeamAsync(Team newTeam)
        {
            await _teamRepository.AddAsync(_mapper.Map<DataRepository.DataEntities.TeamProfile>(newTeam));
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

        public Task BulkAddTeamsAsync(IEnumerable<Team> newTeams)
        {
            return _teamRepository.BatchUpsertAsync(_mapper.Map<IEnumerable<DataRepository.DataEntities.TeamProfile>>(newTeams));
        }

        public async Task<IEnumerable<TeamSeasonStats>> GetSeasonsTeamsStatsAsync(short year, byte season)
        {
            var stats = await _teamRepository.GetSeasonTeamsStatsAsync(year, season);
            return _mapper.Map<IEnumerable<TeamSeasonStats>>(stats.OrderByDescending(s => s.Pts).ThenByDescending(s => s.GD).ThenByDescending(s => s.GF));
        }

        public async Task<TeamSeasonStats> AddTeamStatsAsync(short year, byte season, int teamId, string teamName)
        {
            var newStats = new DataRepository.DataEntities.TeamSeasonStats(year, season, teamId) {TeamName = teamName};
            await _statsRepository.AddAsync(newStats);
            return _mapper.Map<TeamSeasonStats>(newStats);
        }

        public Task BulkUpsertTeamStats(short year, byte season, IEnumerable<TeamSeasonStats> stats)
        {
            return _statsRepository.BatchUpsertAsync(
                _mapper.Map<IEnumerable<DataRepository.DataEntities.TeamSeasonStats>>(stats, opt =>
                {
                    opt.Items["year"] = year;
                    opt.Items["season"] = season;
                }));
        }
    }
}
