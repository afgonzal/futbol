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
        Task<IEnumerable<TeamSeasonStats>> GetSeasonTeamsStatsAsync(short year, byte season);

        Task<TeamSeasonStats> AddTeamStatsAsync(int teamId, short year, byte season, string teamName);
        Task BulkUpsertTeamStats(short year, byte season, IEnumerable<TeamSeasonStats> stats);
        Task ResetAllTeamStatsFromSeasonAsync(short year, byte season);

        Task<TeamSeasonStats> GetTeamSeasonStatsAsync(int teamId, short year, byte season);
        Task<IEnumerable<Match>> GetTeamSeasonMatchesAsync(int teamId, short year, byte season);

        Task<bool> VerifyTeamSeasonStatsAsync(int teamId, short year, byte season);

        Task<bool> VerifySeasonStatsAsync(short year, byte season);
        Task UpdateTeamStatsAsync(int teamId, short year, byte season, TeamSeasonStats stats);

        Task ReprocessSeasonStatsAsync(short year, byte season);
    }

    public class TeamsService : ITeamsService
    {
        private readonly IMatchesService _matchesService;
        private readonly ITeamRepository _teamRepository;
        private readonly ITeamStatsRepository _statsRepository;
        private readonly IMatchRepository _matchRepository;
        private readonly ISeasonConfigService _configService;
        private readonly IMapper _mapper;

        public TeamsService(IMatchesService matchesService, ITeamRepository teamRepository, ITeamStatsRepository statsRepository,
            IMatchRepository matchRepository, ISeasonConfigService configService, IMapper mapper)
        {
            _matchesService = matchesService;
            _teamRepository = teamRepository;
            _statsRepository = statsRepository;
            _matchRepository = matchRepository;
            _configService = configService;
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
            await _teamRepository.BatchDeleteAsync(teams);
        }

        public Task BulkAddTeamsAsync(IEnumerable<Team> newTeams)
        {
            return _teamRepository.BatchUpsertAsync(_mapper.Map<IEnumerable<DataRepository.DataEntities.TeamProfile>>(newTeams));
        }

        public async Task<IEnumerable<TeamSeasonStats>> GetSeasonTeamsStatsAsync(short year, byte season)
        {
            var stats = await _statsRepository.GetSeasonTeamsStatsAsync(year, season);
            return _mapper.Map<IEnumerable<TeamSeasonStats>>(stats.OrderByDescending(s => s.Pts).ThenByDescending(s => s.GD).ThenByDescending(s => s.GF));
        }

        public async Task<TeamSeasonStats> AddTeamStatsAsync(int teamId, short year, byte season, string teamName)
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

        public async Task ResetAllTeamStatsFromSeasonAsync(short year, byte season)
        {
            var stats = await _statsRepository.GetSeasonTeamsStatsAsync(year, season);
            await _statsRepository.BatchDeleteAsync(stats);
        }

        public async Task<TeamSeasonStats> GetTeamSeasonStatsAsync(int teamId, short year, byte season)
        {
            var stats = await _statsRepository.GetByKeyAsync(year, $"SeasonStats#{season}#{teamId}");
            return _mapper.Map<TeamSeasonStats>(stats);
        }

        public async Task<bool> VerifyTeamSeasonStatsAsync(int teamId, short year, byte season)
        {
            var stats = await GetTeamSeasonStatsAsync(teamId, year, season);
            
            var homeMatches = await _matchRepository.GetTeamSeasonHomeMatches(year, season, teamId);
            var gf = homeMatches.Sum(m => m.HomeScore);
            var ga = homeMatches.Sum(m => m.AwayScore);
            var homeResults = homeMatches.Where(m => m.WasPlayed).Select(m =>
                m.HomeScore > m.AwayScore ? WhoWon.Home : m.AwayScore > m.HomeScore ? WhoWon.Away : WhoWon.Draw).ToList();
            var w = homeResults.Count(r => r == WhoWon.Home);
            var l = homeResults.Count(r => r == WhoWon.Away);
            var d = homeResults.Count(r => r == WhoWon.Draw);

            var awayMatches = await _matchRepository.GetTeamSeasonAwayMatches(year, season, teamId);
            gf += awayMatches.Sum(m => m.AwayScore);
            ga += awayMatches.Sum(m => m.HomeScore);
            var awayResults = awayMatches.Where(m => m.WasPlayed).Select(m =>
                m.HomeScore > m.AwayScore ? WhoWon.Home : m.AwayScore > m.HomeScore ? WhoWon.Away : WhoWon.Draw).ToList();
            w  += awayResults.Count(r => r == WhoWon.Away);
            l += awayResults.Count(r => r == WhoWon.Home);
            d += awayResults.Count(r => r == WhoWon.Draw);

            if (stats != null)
                return (stats.W == w && stats.L == l && stats.D == d && stats.GF == gf && stats.GA == ga);
            
            return w + l + d <= 0;
        }

        public async Task<bool> VerifySeasonStatsAsync(short year, byte season)
        {
            var teams = await GetYearTeamsAsync(year);
            var totals = new SeasonStatsTotals();
            foreach (var team in teams)
            {
                var stats = await GetTeamSeasonStatsAsync(team.Id, year, season);
                totals.G += stats.G;
                totals.W += stats.W;
                totals.L += stats.L;
                totals.D += stats.D;
                totals.GF += stats.GF;
                totals.GA += stats.GA;
            }

            if (totals.GA != totals.GF)
                return false;
            if (totals.W != totals.L)
                return false;
            if (totals.D + totals.L + totals.W != totals.G)
                return false;
            if (totals.D % 2 != 0)
                return false;
            if (totals.G % 2 != 0)
                return false;
            return true;
        }

        public async Task<IEnumerable<Match>> GetTeamSeasonMatchesAsync(int teamId, short year, byte season)
        {
            var matches = await _matchRepository.GetTeamSeasonMatches(year, season, teamId);
            return _mapper.Map<IEnumerable<Match>>(matches.OrderBy(match => match.Round));
        }

        public Task UpdateTeamStatsAsync(int teamId, short year, byte season, TeamSeasonStats stats)
        {
            return _statsRepository.UpdateAsync(_mapper.Map<DataRepository.DataEntities.TeamSeasonStats>(stats, opt =>
            {
                opt.Items["year"] = year;
                opt.Items["season"] = season;
            }));
        }

        public async Task ReprocessSeasonStatsAsync(short year, byte season)
        {
            var stats = (await GetSeasonTeamsStatsAsync(year, season)).ToDictionary(stat => stat.Id, stat => stat);
            foreach (var teamStats in stats)
            {
                teamStats.Value.Reset();
            }


            var seasonConfig = await _configService.GetConfig(year, season);

            for (byte round = 1; round <= seasonConfig.RoundsCount; round++)
            {
                var matches = await _matchesService.GetSeasonRoundMatchesAsync(year, season, round);

                foreach (var match in matches)
                {
                    var newMatchWinner = match.HomeScore > match.AwayScore ? WhoWon.Home :
                        match.AwayScore > match.HomeScore ? WhoWon.Away : WhoWon.Draw;
                    var homeStats = stats[match.HomeTeamId];
                    var awayStats = stats[match.AwayTeamId];
                    homeStats.G++;
                    awayStats.G++;
                    homeStats.GF += match.HomeScore.GetValueOrDefault();
                    homeStats.GA += match.AwayScore.GetValueOrDefault();
                    awayStats.GF += match.AwayScore.GetValueOrDefault();
                    awayStats.GA += match.HomeScore.GetValueOrDefault();

                    switch (newMatchWinner)
                    {
                        case WhoWon.Home:
                            homeStats.W++;
                            awayStats.L++;
                            break;
                        case WhoWon.Away:
                            homeStats.L++;
                            awayStats.W++;
                            break;
                        case WhoWon.Draw:
                            homeStats.D++;
                            awayStats.D++;
                            break;
                    }
                }
            }

            await BulkUpsertTeamStats(year, season, stats.Values);
        }
    }
}
