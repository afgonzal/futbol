using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.BusinessEntities;
using Futbol.Seasons.DataRepository.Repositories;

namespace Futbol.Seasons.Services
{
    public interface IMatchesService
    {
        Task AddMatch(Match newMatch);
        Task DeleteAllMatchesFromSeasonRoundAsync(short year, byte season, byte round);
        Task<IEnumerable<Match>> GetSeasonRoundMatchesAsync(short year, byte season, byte round);
        Task BulkAddMatches(IList<Match> newMatches);
        Task SetRoundResults(short year, byte season, byte round, IList<Match> matchesResults);
    }

    public class MatchesService : IMatchesService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IMapper _mapper;
        public MatchesService(IMatchRepository matchRepository, IMapper mapper)
        {
            _matchRepository = matchRepository;
            _mapper = mapper;
        }

        public async Task AddMatch(Match newMatch)
        {
            await _matchRepository.AddAsync(_mapper.Map<DataRepository.DataEntities.Match>(newMatch));
        }

        public async Task DeleteAllMatchesFromSeasonRoundAsync(short year, byte season, byte round)
        {
            var matches = await _matchRepository.GetMatchesAsync(year, season, round);
            await _matchRepository.DeleteMatchesAsync(matches);
        }

        public async Task<IEnumerable<Match>> GetSeasonRoundMatchesAsync(short year, byte season, byte round)
        {
            var matches = await _matchRepository.GetMatchesAsync(year, season, round);
            return _mapper.Map<IEnumerable<Match>>(matches);
        }

        public Task BulkAddMatches(IList<Match> newMatches)
        {
            //numerate MatchId
            for (byte matchId = 0; matchId < newMatches.Count(); matchId++)
            {
                newMatches[matchId].MatchId = (byte)(matchId+1);
            }

            return _matchRepository.BatchUpsertAsync(_mapper.Map<IEnumerable<DataRepository.DataEntities.Match>>(newMatches));
        }
        /// <summary>
        /// Update result and update stats (no streams)
        /// </summary>
        /// <param name="year"></param>
        /// <param name="season"></param>
        /// <param name="round"></param>
        /// <param name="matchesResults"></param>
        /// <returns></returns>
        public async Task SetRoundResults(short year, byte season, byte round, IList<Match> matchesResults)
        {
            var matches = (await _matchRepository.GetMatchesAsync(year, season, round)).ToDictionary(match => match.HomeTeamId, match => match);

            foreach (var result in matchesResults)
            {
                if (!matches.ContainsKey(result.HomeTeamId))
                {
                    throw new DataException($"Missing match {{year}}#{{season}}#{{round}} {result.HomeTeamId} vs {result.AwayTeamId}.");
                }

                var match = matches[result.HomeTeamId];
                match.HomeScore = result.HomeScore;
                match.AwayScore = result.AwayScore;
            }

            await _matchRepository.BatchUpsertAsync(matches.Values.ToList());
        }
    }
}
