﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Futbol.Seasons.BusinessEntities;
using Futbol.Seasons.Services;
using Newtonsoft.Json;

namespace Futbol.Seasons.StatsStream
{
    public interface ITeamsStatsAggregationService
    {
        Task ProcessStreamRecordAsync(DynamoDBEvent.DynamodbStreamRecord record, ILambdaContext context);
    }
    public class TeamsStatsAggregationService : ITeamsStatsAggregationService
    {
        private readonly ITeamsService _teamsService;
        private readonly IMatchesService _matchesService;


        public TeamsStatsAggregationService(ITeamsService teamsService, IMatchesService matchesService)
        {
            _teamsService = teamsService;
            _matchesService = matchesService;
        }

        private short? _year;
        private byte? _season;

        private void SetYearSeason(short year, byte season)
        {
            if (!_year.HasValue)
            {
                _year = year;
                _season = season;
            }
        }


        public async Task ProcessStreamRecordAsync(DynamoDBEvent.DynamodbStreamRecord record, ILambdaContext context)
        {
            context.Logger.LogLine($"Process Stream Match result keys:{Document.FromAttributeMap(record.Dynamodb.Keys).ToJson()}.");

            if (record.EventName != OperationType.MODIFY)
            {
                context.Logger.LogLine($"Process Stream {record.EventName}.");
                return;
            }

            context.Logger.LogLine($"Process Stream Match old:{Document.FromAttributeMap(record.Dynamodb.OldImage).ToJson()}.");
            context.Logger.LogLine($"Process Stream Match new:{Document.FromAttributeMap(record.Dynamodb.NewImage).ToJson()}.");

            var oldResult = JsonConvert.DeserializeObject<DataRepository.DataEntities.Match>(Document.FromAttributeMap(record.Dynamodb.OldImage).ToJson());
            var newResult = JsonConvert.DeserializeObject<DataRepository.DataEntities.Match>(Document.FromAttributeMap(record.Dynamodb.NewImage).ToJson());


            var homeStats = await _teamsService.GetTeamSeasonStatsAsync(oldResult.HomeTeamId, oldResult.Year, oldResult.Season);
            var awayStats = await _teamsService.GetTeamSeasonStatsAsync(oldResult.AwayTeamId, oldResult.Year, oldResult.Season);

            if (homeStats == null)
            {
                context.Logger.LogLine($"Team stats not found for team {oldResult.HomeTeamId} processing {newResult.Year}#{newResult.Season}#{newResult.Round}#{newResult.MatchId}. \n Stats will be generated.");
                homeStats = await _teamsService.AddTeamStatsAsync(oldResult.HomeTeamId,
                    oldResult.Year, oldResult.Season, oldResult.HomeTeamName);
            }

            if (awayStats == null)
            {
                context.Logger.LogLine($"Team stats not found for team {oldResult.AwayTeamId} processing {newResult.Year}#{newResult.Season}#{newResult.Round}#{newResult.MatchId}. \n Stats will be generated.");
                awayStats = await _teamsService.AddTeamStatsAsync(oldResult.AwayTeamId,
                    oldResult.Year, oldResult.Season, oldResult.AwayTeamName);
            }

            if (!(await ValidateMatchAsync(context.Logger, oldResult, newResult, homeStats, awayStats)))
                throw new DataException("Unable to process record, invalid operation for match.");

            SetYearSeason(newResult.Year, newResult.Season);

            
            if (!oldResult.WasPlayed && newResult.WasPlayed) //just add points
            {
                context.Logger.LogLine($"Set new result {newResult.MatchId}.");
                SetNewResult(newResult, homeStats, awayStats);
                await _teamsService.UpdateTeamStatsAsync(newResult.HomeTeamId, newResult.Year, newResult.Season, homeStats);
                await _teamsService.UpdateTeamStatsAsync(newResult.AwayTeamId, newResult.Year, newResult.Season, awayStats);
            }
            else if (oldResult.WasPlayed && newResult.WasPlayed)//first remove points then add
            {
                context.Logger.LogLine($"Replace result {newResult.MatchId}.");
                RemoveOldResult(oldResult, homeStats, awayStats);
                SetNewResult(newResult, homeStats, awayStats);
                await _teamsService.UpdateTeamStatsAsync(newResult.HomeTeamId, newResult.Year, newResult.Season, homeStats);
                await _teamsService.UpdateTeamStatsAsync(newResult.AwayTeamId, newResult.Year, newResult.Season, awayStats);
            }
            else if (oldResult.WasPlayed && !newResult.WasPlayed) //remove points
            {
                context.Logger.LogLine($"Remove result {newResult.MatchId}.");
                RemoveOldResult(oldResult, homeStats,awayStats);
                await _teamsService.UpdateTeamStatsAsync(newResult.HomeTeamId, newResult.Year, newResult.Season, homeStats);
                await _teamsService.UpdateTeamStatsAsync(newResult.AwayTeamId, newResult.Year, newResult.Season, awayStats);
            }
            else //do nothing, no change
            {
                context.Logger.LogLine($"No change in result {newResult.MatchId}.");
            }
        }


        /// <summary>
        /// Remove points etc from a result
        /// </summary>
        /// <param name="oldResult"></param>
        /// <param name="homeStats"></param>
        /// <param name="awayStats"></param>
        /// <returns></returns>
        private void RemoveOldResult(DataRepository.DataEntities.Match oldResult, TeamSeasonStats homeStats, TeamSeasonStats awayStats)
        {
            var matchWinner = oldResult.HomeScore > oldResult.AwayScore ? WhoWon.Home :
                oldResult.AwayScore > oldResult.HomeScore ? WhoWon.Away : WhoWon.Draw;

            homeStats.G--;
            awayStats.G--;
            homeStats.GF -= oldResult.HomeScore.GetValueOrDefault();
            homeStats.GA -= oldResult.AwayScore.GetValueOrDefault();
            awayStats.GF -= oldResult.AwayScore.GetValueOrDefault();
            awayStats.GA -= oldResult.HomeScore.GetValueOrDefault();

            switch (matchWinner)
            {
                case WhoWon.Home:
                    homeStats.W--;
                    awayStats.L--;
                    break;
                case WhoWon.Away:
                    homeStats.L--;
                    awayStats.W--;
                    break;
                case WhoWon.Draw:
                    homeStats.D--;
                    awayStats.D--;
                    break;
            }
        }

        /// <summary>
        /// Adds points etc to a new result
        /// </summary>
        /// <param name="newResult"></param>
        /// <param name="homeStats"></param>
        /// <param name="awayStats"></param>
        /// <returns></returns>
        private void SetNewResult(DataRepository.DataEntities.Match newResult, TeamSeasonStats homeStats, TeamSeasonStats awayStats)
        {
            var newMatchWinner = newResult.HomeScore > newResult.AwayScore ? WhoWon.Home :
                newResult.AwayScore > newResult.HomeScore ? WhoWon.Away : WhoWon.Draw;

            homeStats.G++;
            awayStats.G++;
            homeStats.GF += newResult.HomeScore.GetValueOrDefault();
            homeStats.GA += newResult.AwayScore.GetValueOrDefault();
            awayStats.GF += newResult.AwayScore.GetValueOrDefault();
            awayStats.GA += newResult.HomeScore.GetValueOrDefault();

            switch (newMatchWinner)
            {
                case WhoWon.Home:
                    homeStats.W++;
                    awayStats.L++;
                    break;
                case WhoWon.Away:
                    homeStats.L++;
                    awayStats.W++;
                    break;
                case WhoWon.Draw:
                    homeStats.D++;
                    awayStats.D++;
                    break;
            }
        }

        /// <summary>
        /// Validate New and Old are valid
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="oldResult"></param>
        /// <param name="newResult"></param>
        /// <param name="homeStats"></param>
        /// <param name="awayStats"></param>
        /// <returns></returns>
        private async Task<bool> ValidateMatchAsync(ILambdaLogger logger, DataRepository.DataEntities.Match oldResult,
            DataRepository.DataEntities.Match newResult, TeamSeasonStats homeStats, TeamSeasonStats awayStats)
        {
            if (oldResult.Year != newResult.Year || oldResult.Season != newResult.Season ||
                oldResult.Round != newResult.Round)
            {
                logger.LogLine($"Match is invalid {newResult.Year}#{newResult.Season}#{newResult.Round}#{newResult.MatchId}.");
                return false;
            }

            if ((_year.HasValue && _season.HasValue) && (newResult.Year != _year.Value || newResult.Season != _season.Value))
            {
                logger.LogLine($"Match has invalid year or season {newResult.Year}#{newResult.Season}#{newResult.Round}#{newResult.MatchId}.");
                return false;
            }

            if (oldResult.MatchId != newResult.MatchId || oldResult.HomeTeamId != newResult.HomeTeamId ||
                oldResult.AwayTeamId != newResult.AwayTeamId)
            {
                logger.LogLine($"Match is invalid, teams don't match {newResult.Year}#{newResult.Season}#{newResult.Round}#{newResult.MatchId}.");
                return false;
            }

            if (newResult.WasPlayed && (!newResult.HomeScore.HasValue || !newResult.AwayScore.HasValue))
            {
                logger.LogLine($"Match is invalid, match played but no score {newResult.Year}#{newResult.Season}#{newResult.Round}#{newResult.MatchId}.");
                return false;
            }

            return true;
        }
    }
}
