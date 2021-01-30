﻿using System;
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
    [Route("api/matches/{year:int}/{season:int}")]
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

        [HttpPost("batchFixture")]
        public async Task<IActionResult> BulkAddFixture([FromRoute] short year, [FromRoute] byte season,
            [FromBody] IList<IList<MatchAddRequest>> newMatches)
        {
            try
            {
                var teams = (await _teamsService.GetYearTeamsAsync(year)).ToDictionary(team => team.Abbreviation.ToLowerInvariant(), team => team.Id);
                foreach (var match in newMatches.SelectMany(m => m.Select(match => match)))
                {
                    if (!teams.ContainsKey(match.HomeTeamAbbr.ToLowerInvariant()) || !teams.ContainsKey(match.AwayTeamAbbr.ToLowerInvariant()))
                        throw new ArgumentException($"Teams not found for match {match.HomeTeamAbbr}-{match.AwayTeamAbbr}.");
                    match.HomeTeamId = teams[match.HomeTeamAbbr.ToLowerInvariant()];
                    match.AwayTeamId = teams[match.AwayTeamAbbr.ToLowerInvariant()];
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error matching teams.", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error matching teams.");
            }
            try
            {

                byte round = 1;
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
    }
}
