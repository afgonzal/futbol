using System.Collections.Generic;
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
            for (int matchId = 0; matchId < newMatches.Count(); matchId++)
            {
                newMatches[matchId].MatchId = matchId + 1;
            }

            return _matchRepository.BatchAddAsync(_mapper.Map<IEnumerable<DataRepository.DataEntities.Match>>(newMatches));
        }
    }
}
