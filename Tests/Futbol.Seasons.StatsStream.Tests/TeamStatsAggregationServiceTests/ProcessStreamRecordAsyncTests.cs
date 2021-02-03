using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Futbol.Seasons.BusinessEntities;
using Futbol.Seasons.Services;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Futbol.Seasons.StatsStream.Tests.TeamStatsAggregationServiceTests
{
    [TestFixture]
    public class ProcessStreamRecordAsyncTests
    {
        private Mock<ITeamsService> _teamsService;
        private Mock<IMatchesService> _matchesService;
        private Mock<ILambdaContext> _context;
        private Mock<ILambdaLogger> _logger;
        private const short Year = 2020;
        private const byte Season = 2;
        private const byte Round = 1;
        private const byte MatchId = 3;

        [SetUp]
        public void SetUp()
        {
            _teamsService = new Mock<ITeamsService>();
            _matchesService = new Mock<IMatchesService>();
            _context = new Mock<ILambdaContext>();
            _logger = new Mock<ILambdaLogger>();
            _context.Setup(x => x.Logger).Returns(_logger.Object);
        }

        [Test]
        public async Task ReplaceResult_AwayWin_Success()
        {
            _teamsService.Setup(x => x.GetSeasonsTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedStats().ToList());
            var service = new TeamsStatsAggregationService(_teamsService.Object, _matchesService.Object, _context.Object);
            await service.ProcessStreamRecordAsync(MockedRecord(OldMatch(2, 1, true), NewMatch(2, 3)), _context.Object);

            _logger.Verify(x => x.LogLine(It.IsAny<string>()), Times.Exactly(1));
            _teamsService.Verify(
                x => x.AddTeamStatsAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
            _teamsService.Verify(x => x.GetSeasonsTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()), Times.Once);
        }

        [Test]
        public async Task RemoveResult_AwayWin_Success()
        {
            _teamsService.Setup(x => x.GetSeasonsTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedStats().ToList());
            var service = new TeamsStatsAggregationService(_teamsService.Object, _matchesService.Object, _context.Object);
            await service.ProcessStreamRecordAsync(MockedRecord(OldMatch(2, 1, true), NewMatch(2, 3, false)),
                _context.Object);

            _logger.Verify(x => x.LogLine(It.IsAny<string>()), Times.Exactly(1));
            _teamsService.Verify(
                x => x.AddTeamStatsAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
            _teamsService.Verify(x => x.GetSeasonsTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()), Times.Once);
        }

        [Test]
        public async Task NewResult_AwayWin_Success()
        {
            _teamsService.Setup(x => x.GetSeasonsTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedStats().ToList());
            var service = new TeamsStatsAggregationService(_teamsService.Object, _matchesService.Object, _context.Object);
            await service.ProcessStreamRecordAsync(MockedRecord(OldMatch(2, 1), NewMatch(2, 3)), _context.Object);

            _logger.Verify(x => x.LogLine(It.IsAny<string>()), Times.Exactly(1));
            _teamsService.Verify(
                x => x.AddTeamStatsAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
            _teamsService.Verify(x => x.GetSeasonsTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()), Times.Once);
        }

        [Test]
        public async Task NewResult_HomeWin_Success()
        {
            _teamsService.Setup(x => x.GetSeasonsTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedStats().ToList());
            var service = new TeamsStatsAggregationService(_teamsService.Object, _matchesService.Object, _context.Object);
            await service.ProcessStreamRecordAsync(MockedRecord(OldMatch(2, 1), NewMatch(4, 3)), _context.Object);

            _logger.Verify(x => x.LogLine(It.IsAny<string>()), Times.Exactly(1));
            _teamsService.Verify(
                x => x.AddTeamStatsAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
            _teamsService.Verify(x => x.GetSeasonsTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()), Times.Once);
        }

        [Test]
        public async Task NewResult_Draw_Success()
        {
            _teamsService.Setup(x => x.GetSeasonsTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedStats().ToList());
            var service = new TeamsStatsAggregationService(_teamsService.Object, _matchesService.Object, _context.Object);
            await service.ProcessStreamRecordAsync(MockedRecord(OldMatch(2, 1), NewMatch(2, 2)), _context.Object);

            _logger.Verify(x => x.LogLine(It.IsAny<string>()), Times.Exactly(1));
            _teamsService.Verify(
                x => x.AddTeamStatsAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
            _teamsService.Verify(x => x.GetSeasonsTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()), Times.Once);
        }


        [Test]
        public void GetStatsError_ThrowException()
        {
            _teamsService.Setup(x => x.GetSeasonsTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()))
                .ThrowsAsync(new DataException());

            var service = new TeamsStatsAggregationService(_teamsService.Object, _matchesService.Object, _context.Object);
            Assert.ThrowsAsync<DataException>(async () =>
                await service.ProcessStreamRecordAsync(MockedRecord(OldMatch(2, 1), NewMatch(2, 3)), _context.Object));

            _logger.Verify(x => x.LogLine(It.IsAny<string>()), Times.Exactly(1));
            _teamsService.Verify(
                x => x.AddTeamStatsAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
            _teamsService.Verify(x => x.GetSeasonsTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()), Times.Once);
        }

        [Test]
        public void AddNewHomeTeamStatError_ThrowException()
        {
            var missingStat = MockedStats().ToList();
            missingStat.RemoveAt(2);

            _teamsService.Setup(x => x.GetSeasonsTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()))
                .ReturnsAsync(missingStat);
            _teamsService.Setup(x =>
                    x.AddTeamStatsAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<int>(), It.IsAny<string>()))
                .ThrowsAsync(new DataException());

            var service = new TeamsStatsAggregationService(_teamsService.Object, _matchesService.Object, _context.Object);
            Assert.ThrowsAsync<DataException>(async () =>
                await service.ProcessStreamRecordAsync(MockedRecord(OldMatch(2, 1), NewMatch(2, 3)), _context.Object));

            _logger.Verify(x => x.LogLine(It.IsAny<string>()), Times.Exactly(2));
            _teamsService.Verify(
                x => x.AddTeamStatsAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Once);
            _teamsService.Verify(x => x.GetSeasonsTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()), Times.Once);
        }

        [Test]
        public void AddAwayHomeTeamStatError_ThrowException()
        {
            var missingStat = MockedStats().ToList();
            missingStat.RemoveAt(2);
            missingStat.RemoveAt(3);

            _teamsService.Setup(x => x.GetSeasonsTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()))
                .ReturnsAsync(missingStat);
            _teamsService
                .SetupSequence(x =>
                    x.AddTeamStatsAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(MockedNewStats(3, "DC United"))
                .ThrowsAsync(new DataException());

            var service = new TeamsStatsAggregationService(_teamsService.Object, _matchesService.Object, _context.Object);
            Assert.ThrowsAsync<DataException>(async () =>
                await service.ProcessStreamRecordAsync(MockedRecord(OldMatch(2, 1), NewMatch(2, 3)), _context.Object));

            _logger.Verify(x => x.LogLine(It.IsAny<string>()), Times.Exactly(3));
            _teamsService.Verify(
                x => x.AddTeamStatsAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Exactly(2));
            _teamsService.Verify(x => x.GetSeasonsTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()), Times.Once);
        }

        private string OldMatch(int homeScore, int awayScore, bool wasPlayed = false)
        {
            return JsonConvert.SerializeObject(new DataRepository.DataEntities.Match
            {
                YearSeasonRound = $"{Year}#{Season}#{Round}", HomeTeamId = 3, AwayTeamId = 5,
                HomeScore = (byte) homeScore,
                AwayScore = (byte) awayScore, MatchId = 3, WasPlayed = wasPlayed,
                HomeTeamName = "DC United",
                AwayTeamName = "LA Galaxy",
                ScheduledDate = DateTimeOffset.Now.AddDays(-1).ToString("O")
            });
        }

        private string NewMatch(int homeScore, int awayScore, bool wasPlayed = true)
        {
            return JsonConvert.SerializeObject(new DataRepository.DataEntities.Match
            {
                YearSeasonRound = $"{Year}#{Season}#{Round}",
                HomeTeamId = 3,
                AwayTeamId = 5,
                HomeTeamName = "DC United",
                AwayTeamName = "LA Galaxy",
                HomeScore = (byte) homeScore,
                AwayScore = (byte) awayScore,
                MatchId = MatchId,
                WasPlayed = wasPlayed,
                ScheduledDate = DateTimeOffset.Now.AddDays(-1).ToString("O")
            });
        }

        private string KeysJson()
        {
            return JsonConvert.SerializeObject(new {YearSeasonRound = $"{Year}#{Season}#{Round}", MatchId = MatchId});
        }

        private DynamoDBEvent.DynamodbStreamRecord MockedRecord(string oldMatchJson, string newMatchJson)
        {
            return new DynamoDBEvent.DynamodbStreamRecord
            {
                Dynamodb = new StreamRecord
                {
                    NewImage = Document.FromJson(newMatchJson).ToAttributeMap(),
                    OldImage = Document.FromJson(oldMatchJson).ToAttributeMap(),
                    Keys = Document.FromJson(KeysJson()).ToAttributeMap()
                }
            };
        }

        private IEnumerable<TeamSeasonStats> MockedStats()
        {
            return Enumerable.Range(1, 5).Select(tId => new TeamSeasonStats
            {
                Id = tId,
                Name = $"team{tId}",
                G = 5,
                W = (byte) tId,
                L = (byte) (5 - tId),
                GF = (byte) tId,
                GA = (byte) tId
            });
        }

        private TeamSeasonStats MockedNewStats(int teamId, string name)
        {
            return new TeamSeasonStats {Id = teamId, Name = name};
        }
    }
}
