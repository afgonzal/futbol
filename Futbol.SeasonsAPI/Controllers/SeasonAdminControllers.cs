using System;
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
        private readonly ILogger<TeamsController> _logger;

        public SeasonAdminControllers(ITeamsService teamsService, ILogger<TeamsController> logger)
        {
            _teamsService = teamsService;
            _logger = logger;
        }
        [HttpDelete("cleanSeason/{year:int}")]
        public async Task<IActionResult> DeleteSeason([FromRoute]short year)
        {
            
            try
            {
                await _teamsService.DeleteAllTeamsFromSeasonAsync(year);
                _logger.LogDebug($"Teams from {year} deleted." );
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
