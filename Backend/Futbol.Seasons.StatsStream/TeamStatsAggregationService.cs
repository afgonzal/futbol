using System.Collections.Generic;
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
        Task UpdateStatsAsync(ILambdaContext context);
    }
    public class TeamsStatsAggregationService : ITeamsStatsAggregationService
    {
        private readonly ITeamsService _teamsService;
        private readonly IMatchesService _matchesService;
        private readonly ILambdaContext _context;


        public TeamsStatsAggregationService(ITeamsService teamsService, IMatchesService matchesService, ILambdaContext context)
        {
            _teamsService = teamsService;
            _matchesService = matchesService;
            _context = context;
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

        private IDictionary<int, TeamSeasonStats> _stats;

        private async Task<IDictionary<int, TeamSeasonStats>> StatsAsync()
        {
            if (_stats == null)
            {
                var stats = await _teamsService.GetSeasonsTeamsStatsAsync(_year.GetValueOrDefault(), _season.GetValueOrDefault());
                _stats = stats.ToDictionary(team => team.Id, team => team);
                _context.Logger.LogLine("Getting Stats");
                return _stats;
            }
            else return _stats;
        }

        private enum WhoWon
        {
            Home, Away, Draw
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

            if (!(await ValidateMatchAsync(context.Logger, oldResult, newResult)))
                throw new DataException("Unable to process record, invalid operation for match.");

            SetYearSeason(newResult.Year, newResult.Season);

            

            if (!oldResult.WasPlayed && newResult.WasPlayed) //just add points
            {
                context.Logger.LogLine($"Set new result {newResult.MatchId}.");
                await SetNewResult(newResult);
            }
            else if (oldResult.WasPlayed && newResult.WasPlayed)//first remove points then add
            {
                context.Logger.LogLine($"Replace result {newResult.MatchId}.");
                await RemoveOldResult(oldResult);
                await SetNewResult(newResult);
            }
            else if (oldResult.WasPlayed && !newResult.WasPlayed) //remove points
            {
                context.Logger.LogLine($"Remove result {newResult.MatchId}.");
                await RemoveOldResult(oldResult);
            }
            else //do nothing, no change
            {
                context.Logger.LogLine($"No change in result {newResult.MatchId}.");
            }
        }

        public async Task UpdateStatsAsync(ILambdaContext context)
        {
            if (_year.HasValue && _season.HasValue)
            {
                context.Logger.LogLine("Stats updated");
                await _teamsService.BulkUpsertTeamStats(_year.Value, _season.Value, _stats.Values.ToList());
                _stats = null;
            }
            context.Logger.LogLine("No stats to update");
        }

        /// <summary>
        /// Remove points etc from a result
        /// </summary>
        /// <param name="oldResult"></param>
        /// <returns></returns>
        private async Task RemoveOldResult(DataRepository.DataEntities.Match oldResult)
        {
            var matchWinner = oldResult.HomeScore > oldResult.AwayScore ? WhoWon.Home :
                oldResult.AwayScore > oldResult.HomeScore ? WhoWon.Away : WhoWon.Draw;

            var stats = await StatsAsync();
            var homeStats = stats[oldResult.HomeTeamId];
            var awayStats = stats[oldResult.AwayTeamId];
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
        /// <returns></returns>
        private async Task SetNewResult(DataRepository.DataEntities.Match newResult)
        {
            var newMatchWinner = newResult.HomeScore > newResult.AwayScore ? WhoWon.Home :
                newResult.AwayScore > newResult.HomeScore ? WhoWon.Away : WhoWon.Draw;

            var stats = await StatsAsync();
            var homeStats = stats[newResult.HomeTeamId];
            var awayStats = stats[newResult.AwayTeamId];
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
        /// <returns></returns>
        private async Task<bool> ValidateMatchAsync(ILambdaLogger logger, DataRepository.DataEntities.Match oldResult,
            DataRepository.DataEntities.Match newResult)
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

            if (!(await StatsAsync()).ContainsKey(oldResult.HomeTeamId)) 
            {
                logger.LogLine($"Team stats not found for team {oldResult.HomeTeamId} processing {newResult.Year}#{newResult.Season}#{newResult.Round}#{newResult.MatchId}. \n Stats will be generated.");
                var newStat = await _teamsService.AddTeamStatsAsync(oldResult.Year, oldResult.Season, oldResult.HomeTeamId,
                    oldResult.HomeTeamName);
                _stats.Add(oldResult.HomeTeamId, newStat);
            }

            if (!(await StatsAsync()).ContainsKey(oldResult.AwayTeamId))
            {
                logger.LogLine($"Team stats not found for team {oldResult.AwayTeamId} processing {newResult.Year}#{newResult.Season}#{newResult.Round}#{newResult.MatchId}. \n Stats will be generated.");
                var newStat = await _teamsService.AddTeamStatsAsync(oldResult.Year, oldResult.Season, oldResult.AwayTeamId,
                    oldResult.AwayTeamName);
                _stats.Add(oldResult.AwayTeamId, newStat);
            }

            return true;
        }
    }
}
