using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Futbol.Seasons.BusinessEntities;
using Futbol.Seasons.Services;
using Microsoft.Extensions.Logging;

namespace Futbol.SeasonsAPI.Controllers
{
    [Route("api/conference/{year:int}")]
    [ApiController]
    public class ConferenceController : ControllerBase
    {
        private readonly ITeamsService _teamsService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public ConferenceController(ITeamsService teamsService, IMapper mapper, ILogger<ConferenceController> logger)
        {
            _teamsService = teamsService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("{conference:int}/stats")]
        public async Task<IActionResult> GetConferenceTeamStats([FromRoute] short year, [FromRoute] byte conference)
        {
            var stats = await _teamsService.GetConferenceTeamsStatsAsync(year, conference);
            return Ok(_mapper.Map<IEnumerable<TeamConferenceStats>>(stats));
        }
    }
}
