﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
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
    [Route("api/teams")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly ITeamsService _teamsService;
        private readonly IMapper _mapper;
        private readonly ILogger<TeamsController> _logger;

        public TeamsController(ITeamsService teamsService, IMapper mapper, ILogger<TeamsController> logger)
        {
            _teamsService = teamsService;
            _mapper = mapper;
            _logger = logger;
        }
        [HttpPost]
        public async Task<IActionResult> Add([FromBody]TeamAddRequest newTeam)
        {
            if (newTeam == null)
                return BadRequest("Team is required.");

            try
            {
                await _teamsService.AddTeamAsync(_mapper.Map<Team>(newTeam));
                _logger.LogDebug($"Team Created:{newTeam.Year}#{newTeam.Id}" );
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating team {newTeam.Year}#{newTeam.Id}.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding team.");
            }
        }
        [HttpGet("year/{year:int}")]
        public async Task<IActionResult> GetYearTeams([FromRoute]short year)
        {
            try
            {
                var teams = await _teamsService.GetYearTeamsAsync(year);
                return Ok(_mapper.Map<IEnumerable<TeamModel>>(teams));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting teams for year {year}.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error getting teams.");
            }
        }
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkAdd([FromBody]IEnumerable<TeamAddRequest> newTeams)
        {
            try
            {
                await _teamsService.BulkAddTeams(_mapper.Map<IEnumerable<Team>>(newTeams));
                _logger.LogDebug($"Teams Created: {newTeams.Count()} \n {JsonConvert.SerializeObject(newTeams.Select(team => new {Year = team.Year, Name = team.Name}))}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error bulk adding teams.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding teams.");
            }
        }
    }
}
