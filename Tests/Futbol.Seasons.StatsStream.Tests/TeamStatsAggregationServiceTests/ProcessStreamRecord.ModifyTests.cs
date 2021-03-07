using System;
using System.Data;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
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
    public partial class ProcessStreamRecordTests
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
            _teamsService.Setup(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedStats());
            var service = new TeamsStatsAggregationService(_teamsService.Object, _matchesService.Object);
            await service.ProcessStreamRecordAsync(MockedRecord(OldMatch(2, 1, true), NewMatch(2, 3)), _context.Object);

            _logger.Verify(x => x.LogLine(It.IsAny<string>()), Times.Exactly(4));
            _teamsService.Verify(
                x => x.AddTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<string>()),
                Times.Never);
            _teamsService.Verify(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()), Times.Exactly(2));
            _teamsService.Verify(x => x.UpdateTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<TeamStats>()), Times.Exactly(2));
        }

        [Test]
        public async Task RemoveResult_AwayWin_Success()
        {
            _teamsService.Setup(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedStats());
            var service = new TeamsStatsAggregationService(_teamsService.Object, _matchesService.Object);
            await service.ProcessStreamRecordAsync(MockedRecord(OldMatch(2, 1, true), NewMatch(2, 3, false)),
                _context.Object);

            _logger.Verify(x => x.LogLine(It.IsAny<string>()), Times.Exactly(4));
            _teamsService.Verify(
                x => x.AddTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<string>()),
                Times.Never);
            _teamsService.Verify(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()), Times.Exactly(2));
            _teamsService.Verify(x => x.UpdateTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<TeamStats>()), Times.Exactly(2));
        }

        [Test]
        public async Task NewResult_AwayWin_Success()
        {
            _teamsService.Setup(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedStats());
            var service = new TeamsStatsAggregationService(_teamsService.Object, _matchesService.Object);
            await service.ProcessStreamRecordAsync(MockedRecord(OldMatch(2, 1), NewMatch(2, 3)), _context.Object);

            _logger.Verify(x => x.LogLine(It.IsAny<string>()), Times.Exactly(4));
            _teamsService.Verify(
                x => x.AddTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<string>()),
                Times.Never);
            _teamsService.Verify(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()), Times.Exactly(2));
            _teamsService.Verify(x => x.UpdateTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<TeamStats>()), Times.Exactly(2));
        }

        [Test]
        public async Task SameResult_NotPlayed_Success()
        {
            _teamsService.Setup(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedStats());
            var service = new TeamsStatsAggregationService(_teamsService.Object, _matchesService.Object);
            await service.ProcessStreamRecordAsync(MockedRecord(NewMatch(2, 1, false), NewMatch(2,1, false)), _context.Object);

            _logger.Verify(x => x.LogLine(It.IsAny<string>()), Times.Exactly(4));
            _teamsService.Verify(
                x => x.AddTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<string>()),
                Times.Never);
            _teamsService.Verify(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()), Times.Exactly(2));
            _teamsService.Verify(x => x.UpdateTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<TeamStats>()), Times.Never);
        }

        [Test]
        public async Task NewResult_HomeWin_Success()
        {
            _teamsService.Setup(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedStats());
            var service = new TeamsStatsAggregationService(_teamsService.Object, _matchesService.Object);
            await service.ProcessStreamRecordAsync(MockedRecord(OldMatch(2, 1), NewMatch(4, 3)), _context.Object);

            _logger.Verify(x => x.LogLine(It.IsAny<string>()), Times.Exactly(4));
            _teamsService.Verify(
                x => x.AddTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<string>()),
                Times.Never);
            _teamsService.Verify(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()), Times.Exactly(2));
            _teamsService.Verify(x => x.UpdateTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<TeamStats>()), Times.Exactly(2));
        }

        [Test]
        public async Task NewResult_Draw_Success()
        {
            _teamsService.Setup(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedStats());
            var service = new TeamsStatsAggregationService(_teamsService.Object, _matchesService.Object);
            await service.ProcessStreamRecordAsync(MockedRecord(OldMatch(2, 1), NewMatch(2, 2)), _context.Object);

            _logger.Verify(x => x.LogLine(It.IsAny<string>()), Times.Exactly(4));
            _teamsService.Verify(
                x => x.AddTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<string>()),
                Times.Never);
            _teamsService.Verify(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()), Times.Exactly(2));
            _teamsService.Verify(x => x.UpdateTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<TeamStats>()), Times.Exactly(2));
        }


        [Test]
        public void GetStatsError_ThrowException()
        {
            _teamsService.Setup(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()))
                .ThrowsAsync(new DataException());

            var service = new TeamsStatsAggregationService(_teamsService.Object, _matchesService.Object);
            Assert.ThrowsAsync<DataException>(async () =>
                await service.ProcessStreamRecordAsync(MockedRecord(OldMatch(2, 1), NewMatch(2, 3)), _context.Object));

            _logger.Verify(x => x.LogLine(It.IsAny<string>()), Times.Exactly(3));
            _teamsService.Verify(
                x => x.AddTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<string>()),
                Times.Never);
            _teamsService.Verify(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()), Times.Once);
            _teamsService.Verify(x => x.UpdateTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<TeamStats>()), Times.Never);
        }

        [Test]
        public void AddNewHomeTeamStatError_ThrowException()
        {
            _teamsService.SetupSequence(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>())).ReturnsAsync((TeamStats)null)
                .ReturnsAsync(MockedStats());
            _teamsService.Setup(x =>
                    x.AddTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<string>()))
                .ThrowsAsync(new DataException());

            var service = new TeamsStatsAggregationService(_teamsService.Object, _matchesService.Object);
            Assert.ThrowsAsync<DataException>(async () =>
                await service.ProcessStreamRecordAsync(MockedRecord(OldMatch(2, 1), NewMatch(2, 3)), _context.Object));

            _logger.Verify(x => x.LogLine(It.IsAny<string>()), Times.Exactly(4));
            _teamsService.Verify(
                x => x.AddTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<string>()),
                Times.Once);
            _teamsService.Verify(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()), Times.Exactly(2));
            _teamsService.Verify(x => x.UpdateTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<TeamStats>()), Times.Never);
        }

        [Test]
        public void AddAwayTeamStatError_ThrowException()
        {
            _teamsService.SetupSequence(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedStats()).ReturnsAsync((TeamStats)null);
            _teamsService
                .Setup(x =>
                    x.AddTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<string>()))
                .ThrowsAsync(new DataException());

            var service = new TeamsStatsAggregationService(_teamsService.Object, _matchesService.Object);
            Assert.ThrowsAsync<DataException>(async () =>
                await service.ProcessStreamRecordAsync(MockedRecord(OldMatch(2, 1), NewMatch(2, 3)), _context.Object));

            _logger.Verify(x => x.LogLine(It.IsAny<string>()), Times.Exactly(4));
            _teamsService.Verify(
                x => x.AddTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<string>()),
                Times.Once);
            _teamsService.Verify(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()), Times.Exactly(2));
            _teamsService.Verify(x => x.UpdateTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<TeamStats>()), Times.Never);
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
                    NewImage = newMatchJson != null ? Document.FromJson(newMatchJson).ToAttributeMap() : null,
                    OldImage = oldMatchJson != null ? Document.FromJson(oldMatchJson).ToAttributeMap() : null,
                    Keys = Document.FromJson(KeysJson()).ToAttributeMap(),
                },
                EventName = OperationType.MODIFY
            };
        }

        private TeamStats MockedStats()
        {
            return new TeamStats
            {
                Id = 2,
                Name = $"team",
                G = 5,
                W = (byte) 3,
                L = (byte) 4,
                GF = (byte) 5,
                GA = (byte) 6
            };
        }

        private TeamStats MockedNewStats(int teamId, string name)
        {
            return new TeamStats {Id = teamId, Name = name};
        }
    }
}
