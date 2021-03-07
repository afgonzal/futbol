using System;
using System.Linq;
using System.Threading.Tasks;
using Futbol.Seasons.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Futbol.SeasonsAPI.Controllers
{
    [Route("api/season/admin")]
    [ApiController]
    public class SeasonAdminControllers : ControllerBase
    {
        private readonly ITeamsService _teamsService;
        private readonly IMatchesService _matchesService;
        private readonly ILogger<TeamsController> _logger;

        public SeasonAdminControllers(ITeamsService teamsService,IMatchesService matchesService,  ILogger<TeamsController> logger)
        {
            _teamsService = teamsService;
            _matchesService = matchesService;
            _logger = logger;
        }
        [HttpDelete("{year:int}/clean")]
        public async Task<IActionResult> DeleteSeason([FromRoute]short year)
        {
            
            try
            {
                await _teamsService.DeleteAllTeamsFromSeasonAsync(year);
                _logger.LogDebug($"Teams from {year} deleted." );
                //TODO get seasons and rounds
                byte season = 2;
                for (byte round = 1; round < 5; round++)
                {
                    await _matchesService.DeleteAllMatchesFromSeasonRoundAsync(year, season, round);
                    _logger.LogDebug($"Matches from {year}#{season}#{round} deleted.");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting season {year}.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error cleaning season {year}.");
            }
        }

        [HttpPut("{year:int}/{season:int}/reset")]
        public async Task<IActionResult> ResetSeason([FromRoute] short year, [FromRoute] byte season)
        {
            try
            {
                await _teamsService.ResetAllTeamStatsFromSeasonAsync(year, season);
                _logger.LogDebug($"Resetting stats from {year}#{season}.");

                await _matchesService.ResetAllMatchesFromSeasonAsync(year, season);
                _logger.LogDebug($"Resetting matches from {year}#{season}.");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error resetting season {year}#{season}.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error resetting season {year}#{season}.");
            }
        }

        [HttpGet("{year:int}/{season:int}/verify")]
        public async Task<IActionResult> VerifySeason([FromRoute] short year, [FromRoute] byte season)
        {
            try
            {
                var teams = await _teamsService.GetYearTeamsAsync(year);
                
                foreach (var team in teams)
                {
                    var isValid = await _teamsService.VerifyTeamSeasonStatsAsync(team.Id, year, season);
                    if (!isValid)
                        return Ok(new {Valid = false, Team = team.Id});
                }

                var isSeasonValid = await _teamsService.VerifySeasonStatsAsync(year, season);
                if (!isSeasonValid)
                    return Ok(new { Valid = false, Season = season });

                //now validate that every team fixture is same amount of matches
                isSeasonValid = await _teamsService.VerifySeasonFixtureAsync(year, season);
                
                if (!isSeasonValid)
                    return Ok(new { Valid = false, Season = season });
                return Ok(new {Valid = true});
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error verifying season {year}#{season}.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error verifying season {year}#{season}.");
            }
        }

        [HttpPut("{year:int}/{season:int}/reprocess")]
        public async Task<IActionResult> ReprocessSeason([FromRoute] short year, [FromRoute] byte season)
        {
            try
            {
                await _teamsService.ReprocessSeasonStatsAsync(year, season);
                _logger.LogInformation($"Season reprocessed {year}#{season}.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reprocessing season {year}#{season}.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error reprocessing season {year}#{season}.");
            }
        }


        [HttpPut("{year:int}/conference/reprocess")]
        public async Task<IActionResult> ReprocessConferences([FromRoute] short year)
        {
            try
            {
                await _teamsService.ReprocessConferencesStatsAsync(year);
                _logger.LogInformation($"Conferences reprocessed {year}.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reprocessing conferences {year}.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error reprocessing conferences {year}.");
            }
        }
    }
}
