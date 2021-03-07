using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.DataRepository.DataEntities;
using Futbol.Seasons.DataRepository.Repositories;
using Moq;
using NUnit.Framework;

namespace Futbol.Seasons.Services.Tests.TeamsServiceTests
{
    [TestFixture]
    public class GetTeamsStatsTests
    {
        private Mock<ITeamStatsRepository> _repository;
        private IMapper _mapper;
        private const short Year = 2020;
        private const byte Season = 2;
        private const int TeamId = 10;
        [SetUp]
        public void SetUp()
        {
            _repository = new Mock<ITeamStatsRepository>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new EntitiesMappingProfile());
            });
            _mapper = mockMapper.CreateMapper();
        }

        [Test]
        public async Task Ok_Success()
        {
            _repository.Setup(x => x.GetByKeyAsync(It.IsAny<short>(), It.IsAny<string>())).ReturnsAsync(MockedStats());
            var service = new TeamsService(null, null, null, _repository.Object, null, null,  _mapper);
            var result = await service.GetTeamSeasonStatsAsync(TeamId, Year, Season);

            Assert.NotNull(result);
            Assert.IsInstanceOf<BusinessEntities.TeamStats>(result);
            
            _repository.Verify(x => x.GetByKeyAsync(It.IsAny<short>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void RepositoryError_ThrowException()
        {
            _repository.Setup(x => x.GetByKeyAsync(It.IsAny<short>(), It.IsAny<string>())).ThrowsAsync(new DataException());

            var service = new TeamsService(null, null, null, _repository.Object, null, null,  _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.GetTeamSeasonStatsAsync(TeamId, Year, Season));

            _repository.Verify(x => x.GetByKeyAsync(It.IsAny<short>(), It.IsAny<string>()), Times.Once);
        }

        private TeamSeasonStats MockedStats()
        {
            return new TeamSeasonStats(Year, Season, TeamId)
            {
                TeamName = $"team", ConferenceId = 0, G = 5,W = (byte)3, L = (byte)5, GF = (byte)10, GA = (byte)2
            };
        }
    }
}
