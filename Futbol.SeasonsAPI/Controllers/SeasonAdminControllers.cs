﻿using System;
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
        [HttpDelete("cleanSeason/{year:int}")]
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
       
    }
}
