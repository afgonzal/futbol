using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.BusinessEntities;
using Futbol.Seasons.Services;
using Futbol.SeasonsAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Futbol.SeasonsAPI.Controllers
{
    [Route("api/season")]
    [ApiController]
    public class SeasonController : ControllerBase
    {
        private readonly IMatchesService _matchesService;
        private readonly ISeasonConfigService _seasonConfigService;
        private readonly ITeamsService _teamsService;
        private readonly IMapper _mapper;
        private readonly ILogger<SeasonController> _logger;

        public SeasonController(IMatchesService matchesService, ISeasonConfigService seasonConfigService, ITeamsService teamsService, IMapper mapper, ILogger<SeasonController> logger)
        {
            _matchesService = matchesService;
            _seasonConfigService = seasonConfigService;
            _teamsService = teamsService;
            _mapper = mapper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> CurrentSeason()
        {
            var currentSeason = await _matchesService.GetCurrentSeason((short)DateTime.Now.Year);
            if (currentSeason == null)
                return NotFound();
            var seasonConfig = await _seasonConfigService.GetConfig(currentSeason.Year, currentSeason.Season);
            var championshipConfig = await _seasonConfigService.GetConfig(currentSeason.Year);
            var seasonInChampionship = championshipConfig.Seasons.SingleOrDefault(s => s.SeasonId == currentSeason.Season);
            var result = new CurrentSeasonInfo
            {
                Year = currentSeason.Year, Season = seasonConfig.Name, NextRound = currentSeason.NextRound,
                LastRound = currentSeason.LastRound,
                Championship = seasonInChampionship != null ? championshipConfig.Name : null, 
                SeasonId = currentSeason.Season
            };
            return Ok(result);
        }

        [HttpGet("stats/{year:int}/{season:int}")]
        public async Task<IActionResult> GetSeasonTeamsStats([FromRoute] short year, [FromRoute] byte season)
        {
            try
            {
                var stats = await _teamsService.GetSeasonTeamsStatsAsync(year, season);
                return Ok(_mapper.Map<IEnumerable<TeamSeasonStats>>(stats));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting teams stats {year}#{season}.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error getting team stats.");
            }
        }
    }
}
