using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.DataRepository.DataEntities;
using Futbol.Seasons.DataRepository.Repositories;
using Moq;
using NUnit.Framework;

namespace Futbol.Seasons.Services.Tests.TeamsServiceTests
{
    [TestFixture]
    public class GetSeasonTeamsStatsTests
    {
        private Mock<ITeamRepository> _repository;
        private IMapper _mapper;
        private const short Year = 2020;
        private const byte Season = 2;
        [SetUp]
        public void SetUp()
        {
            _repository = new Mock<ITeamRepository>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new EntitiesMappingProfile());
            });
            _mapper = mockMapper.CreateMapper();
        }

        [Test]
        public async Task Ok_Success()
        {
            _repository.Setup(x => x.GetSeasonTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>())).ReturnsAsync(MockedTeams(Year).ToList());
            var service = new TeamsService(_repository.Object, null, _mapper);
            var result = await service.GetSeasonsTeamsStatsAsync(Year, Season);

            Assert.NotNull(result);
            Assert.IsInstanceOf<IEnumerable<BusinessEntities.TeamSeasonStats>>(result);
            
            _repository.Verify(x => x.GetSeasonTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()), Times.Once);
        }

        [Test]
        public void RepositoryError_ThrowException()
        {
            _repository.Setup(x => x.GetSeasonTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>())).ThrowsAsync(new DataException());

            var service = new TeamsService(_repository.Object, null, _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.GetSeasonsTeamsStatsAsync(Year, Season));

            _repository.Verify(x => x.GetSeasonTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()), Times.Once);
        }

        private IEnumerable<TeamSeasonStats> MockedTeams(short year)
        {
            return Enumerable.Range(1,5).Select(tId => new TeamSeasonStats(year, Season, tId)
            {
                TeamName = $"team{tId}", ConferenceId = 0, G = 5,W = (byte)tId, L = (byte)(5-tId), GF = (byte)tId, GA = (byte)tId
            });
        }
    }
}
