using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
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
        Task ResetAllMatchesFromSeasonAsync(short year, byte season);
        Task MoveRoundAsync(short year, byte season, byte round, DateTimeOffset newDate, bool keepTimes);
        Task<CurrentSeasonConfig> GetCurrentSeason(short year);
    }

    public class MatchesService : IMatchesService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IMapper _mapper;
        private readonly ISeasonConfigService _seasonService;
        private readonly ICurrentSeasonConfigRepository _currentSeasonRepository;

        public MatchesService(IMatchRepository matchRepository, ISeasonConfigService seasonService,
            ICurrentSeasonConfigRepository currentSeasonRepository, IMapper mapper)
        {
            _matchRepository = matchRepository;
            _mapper = mapper;
            _seasonService = seasonService;
            _currentSeasonRepository = currentSeasonRepository;
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
                    throw new DataException($"Missing match {year}#{season}#{round} {result.HomeTeamId} vs {result.AwayTeamId}.");
                }

                var match = matches[result.HomeTeamId];
                match.HomeScore = result.HomeScore;
                match.AwayScore = result.AwayScore;
                match.WasPlayed = result.WasPlayed;
                if (result.ScheduledDate.HasValue)
                    match.ScheduledDate = result.ScheduledDate.Value.ToString("u");
            }

            await _matchRepository.BatchUpsertAsync(matches.Values.ToList());
        }

        public async Task ResetAllMatchesFromSeasonAsync(short year, byte season)
        {
            var config = await _seasonService.GetConfig(year, season);
            if (config == null)
                throw new ArgumentException($"Missing config for {year}#{season}.");

            for (byte round = 1; round <= config.RoundsCount; round++)
            {
                var matches = await _matchRepository.GetMatchesAsync(year, season, round);
                foreach (var match in matches)
                {
                    match.WasPlayed = false;
                    match.HomeScore = null;
                    match.AwayScore = null;
                }

                await _matchRepository.BatchUpsertAsync(matches);
            }
        }

        public async Task MoveRoundAsync(short year, byte season, byte round, DateTimeOffset newDate, bool keepTimes)
        {
            var matches = await _matchRepository.GetMatchesAsync(year, season, round);
            foreach (var match in matches)
            {
                if (!match.WasPlayed) //skip already played games
                {
                    if (keepTimes)
                    {
                        var previousDate = DateTimeOffset.Parse(match.ScheduledDate);
                        var nextDate = new DateTimeOffset(newDate.Year, newDate.Month, newDate.Day, previousDate.Hour,
                            previousDate.Minute, previousDate.Second, previousDate.Offset);
                        match.ScheduledDate = nextDate.ToString("u");
                    }
                    else
                    {
                        match.ScheduledDate = newDate.ToString("u");
                    }
                }
            }

            await _matchRepository.BatchUpsertAsync(matches);
        }

        public async Task<CurrentSeasonConfig> GetCurrentSeason(short year)
        {
            var result = await _currentSeasonRepository.GetCurrentSeason(year);
            return _mapper.Map<CurrentSeasonConfig>(result);
        }
    }
}
