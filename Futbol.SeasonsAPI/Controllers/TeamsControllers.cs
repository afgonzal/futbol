using System;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.BusinessEntities;
using Futbol.Seasons.Services;
using Futbol.SeasonsAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Futbol.SeasonsAPI.Controllers
{
    [Route("api/teams")]
    [ApiController]
    public class TeamsControllers : ControllerBase
    {
        private readonly ITeamsService _teamsService;
        private readonly IMapper _mapper;

        public TeamsControllers(ITeamsService teamsService, IMapper mapper)
        {
            _teamsService = teamsService;
            _mapper = mapper;
        }
        [HttpPost]
        public async Task<IActionResult> Add([FromBody]TeamAddRequest newTeam)
        {
            if (newTeam == null)
                return BadRequest("Team is required.");

            try
            {
                await _teamsService.AddTeamAsync(_mapper.Map<Team>(newTeam));
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding team.");
            }
        }
    }
}
