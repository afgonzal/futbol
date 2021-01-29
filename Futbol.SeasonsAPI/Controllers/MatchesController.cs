using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.BusinessEntities;
using Futbol.Seasons.Services;
using Futbol.SeasonsAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Futbol.SeasonsAPI.Controllers
{
    [Route("api/matches/{year:int}/{season:int}/{round:int}")]
    public class MatchesController : ControllerBase
    {
        private readonly IMatchesService _matchesService;
        private readonly IMapper _mapper;
        private readonly ILogger<MatchesController> _logger;
        public MatchesController(IMatchesService matchesService, IMapper mapper, ILogger<MatchesController> logger)
        {
            _matchesService = matchesService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("batch")]
        public async Task<IActionResult> BulkAdd([FromRoute] short year, [FromRoute] byte season, [FromRoute] byte round,
            [FromBody] IEnumerable<MatchAddRequest> newMatches)
        {
            try
            {
                await _matchesService.BulkAddMatches(_mapper.Map<IEnumerable<Match>>(newMatches,

                    opt =>
                    {
                        opt.Items["year"] = year;
                        opt.Items["season"] = season;
                        opt.Items["round"] = round;
                    }));

                _logger.LogDebug($"Matches Created: {newMatches.Count()} \n {JsonConvert.SerializeObject(newMatches.Select(match => new { match.HomeTeamId, match.AwayTeamId }))}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error bulk adding matches.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding matches.");
            }
        }
    }
}
