using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Futbol.Seasons.BusinessEntities;
using Moq;
using NUnit.Framework;

namespace Futbol.Seasons.StatsStream.Tests.TeamStatsAggregationServiceTests
{
    public partial class ProcessStreamRecordTests
    {
        [Test]
        public async Task InsertResult_Success()
        {
            _teamsService.Setup(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedStats());
            var service = new TeamsStatsAggregationService(_teamsService.Object, _matchesService.Object);
            var record = MockedRecord(null, NewMatch(2, 3));
            record.EventName = OperationType.INSERT;
            await service.ProcessStreamRecordAsync(record, _context.Object);

            _logger.Verify(x => x.LogLine(It.IsAny<string>()), Times.Exactly(3));
            _teamsService.Verify(
                x => x.AddTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<string>()),
                Times.Never);
            _teamsService.Verify(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()),
                Times.Exactly(2));
            _teamsService.Verify(
                x => x.UpdateTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(),
                    It.IsAny<TeamStats>()), Times.Exactly(2));
        }

        [Test]
        public async Task DeleteResult_Success()
        {
            _teamsService.Setup(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedStats());
            var service = new TeamsStatsAggregationService(_teamsService.Object, _matchesService.Object);
            var record = MockedRecord(OldMatch(2, 3, true), null);
            record.EventName = OperationType.REMOVE;
            await service.ProcessStreamRecordAsync(record, _context.Object);

            _logger.Verify(x => x.LogLine(It.IsAny<string>()), Times.Exactly(3));
            _teamsService.Verify(
                x => x.AddTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<string>()),
                Times.Never);
            _teamsService.Verify(x => x.GetTeamSeasonStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>()),
                Times.Exactly(2));
            _teamsService.Verify(
                x => x.UpdateTeamStatsAsync(It.IsAny<int>(), It.IsAny<short>(), It.IsAny<byte>(),
                    It.IsAny<TeamStats>()), Times.Exactly(2));
        }
    }
}