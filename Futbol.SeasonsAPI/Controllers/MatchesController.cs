using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.BusinessEntities;
using Futbol.Seasons.Services;
using Futbol.SeasonsAPI.Models;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Futbol.SeasonsAPI.Controllers
{
    [Route("api/matches/{year:int}")]
    public class MatchesController : ControllerBase
    {
        private readonly IMatchesService _matchesService;
        private readonly ITeamsService _teamsService;
        private readonly IMapper _mapper;
        private readonly ILogger<MatchesController> _logger;
        public MatchesController(IMatchesService matchesService,ITeamsService teamsService,  IMapper mapper, ILogger<MatchesController> logger)
        {
            _matchesService = matchesService;
            _teamsService = teamsService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("{season:int}/{round:int}")]
        public async Task<IActionResult> GetSeasonRoundMatchesAsync([FromRoute] short year, [FromRoute] byte season,
            [FromRoute] byte round)
        {
            try
            {
                var matches = await _matchesService.GetSeasonRoundMatchesAsync(year, season, round);
                return Ok(_mapper.Map<IEnumerable<MatchModel>>(matches));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting teams for {year}:{season}:{round}.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error getting round matches.");
            }
        }

        [HttpPost("{season:int}/batchFixture")]
        public async Task<IActionResult> BulkAddFixture([FromRoute] short year, [FromRoute] byte season,
            [FromBody] IList<IList<MatchAddRequest>> newMatches, [FromQuery]byte startingFromRound = 1)
        {
            try
            {
                var teams = (await _teamsService.GetYearTeamsAsync(year)).ToDictionary(team => team.Abbreviation.ToLowerInvariant(), team => team);

                foreach (var match in newMatches.SelectMany(m => m.Select(match => match)))
                {
                    if (!teams.ContainsKey(match.HomeTeamAbbr.ToLowerInvariant()) || !teams.ContainsKey(match.AwayTeamAbbr.ToLowerInvariant()))
                        throw new ArgumentException($"Teams not found for match {match.HomeTeamAbbr}-{match.AwayTeamAbbr}.");
                    match.HomeTeamId = teams[match.HomeTeamAbbr.ToLowerInvariant()].Id;
                    match.AwayTeamId = teams[match.AwayTeamAbbr.ToLowerInvariant()].Id;
                    match.HomeTeamName = teams[match.HomeTeamAbbr.ToLowerInvariant()].Name;
                    match.AwayTeamName = teams[match.AwayTeamAbbr.ToLowerInvariant()].Name;

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error matching teams.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error matching teams.");
            }
            try
            {

                byte round = startingFromRound;
                foreach (var roundMatches in newMatches)
                {
                    await _matchesService.BulkAddMatches(_mapper.Map<IList<Match>>(roundMatches,

                        opt =>
                        {
                            opt.Items["year"] = year;
                            opt.Items["season"] = season;
                            opt.Items["round"] = round;
                        }));
                    _logger.LogDebug($"{roundMatches.Count} matches Created for {year}:{season}:{round}.");
                    round++;
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error bulk adding matches.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding matches.");
            }
        }

        [HttpPost("{season:int}/{round:int}")]
        public async Task<IActionResult> SetRoundResults([FromRoute] short year, [FromRoute] byte season,[FromRoute]byte round,
           [FromBody] IList<MatchResultRequest> matchesResults)
        {
            try
            {
                var teams = (await _teamsService.GetYearTeamsAsync(year)).ToDictionary(team => team.Abbreviation.ToLowerInvariant(), team => team);
                foreach (var match in matchesResults)
                {
                    if (!teams.ContainsKey(match.HomeTeamAbbr.ToLowerInvariant()) || !teams.ContainsKey(match.AwayTeamAbbr.ToLowerInvariant()))
                        throw new ArgumentException($"Teams not found for match {match.HomeTeamAbbr}-{match.AwayTeamAbbr}.");
                    match.HomeTeamId = teams[match.HomeTeamAbbr.ToLowerInvariant()].Id;
                    match.AwayTeamId = teams[match.AwayTeamAbbr.ToLowerInvariant()].Id;
                    match.HomeTeamName = teams[match.HomeTeamAbbr.ToLowerInvariant()].Name;
                    match.AwayTeamName = teams[match.AwayTeamAbbr.ToLowerInvariant()].Name;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error matching teams.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error matching teams.");
            }
            try
            {
                    await _matchesService.SetRoundResults(year, season, round,_mapper.Map<IList<Match>>(matchesResults,

                        opt =>
                        {
                            opt.Items["year"] = year;
                            opt.Items["season"] = season;
                            opt.Items["round"] = round;
                        }));
                    _logger.LogDebug($"{matchesResults.Count} matches results set for {year}:{season}:{round}.");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error bulk adding matches.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding matches.");
            }
        }

        [HttpPost("{season:int}/{round:int}/moveDate")]
        public async Task<IActionResult> MoveRoundDate([FromRoute] short year, [FromRoute] byte season,
            [FromRoute] byte round, [FromQuery] DateTimeOffset newDate, [FromQuery]bool keepTimes)
        {
            if (keepTimes)
                newDate = newDate.Date;
            await _matchesService.MoveRoundAsync(year, season, round, newDate, keepTimes);
            return Ok();
        }

        [HttpGet("next")]
        public async Task<IActionResult> GetNextRound([FromRoute] short year)
        {
            var currentSeason = await _matchesService.GetCurrentSeason(year);
            if (currentSeason.NextRound.HasValue)
                return await GetSeasonRoundMatchesAsync(currentSeason.Year, currentSeason.Season, currentSeason.NextRound.Value);
            return NotFound();
        }

    }
}
