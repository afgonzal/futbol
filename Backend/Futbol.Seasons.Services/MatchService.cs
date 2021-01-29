using System.Collections.Generic;
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
        Task BulkAddMatches(IEnumerable<Match> newMatches);
    }

    public class MatchService : IMatchesService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IMapper _mapper;
        public MatchService(IMatchRepository matchRepository, IMapper mapper)
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

        public Task BulkAddMatches(IEnumerable<Match> newMatches)
        {
            return _matchRepository.BatchAddAsync(_mapper.Map<IEnumerable<DataRepository.DataEntities.Match>>(newMatches));
        }
    }
}
