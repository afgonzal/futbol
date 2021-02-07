using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.BusinessEntities;
using Futbol.Seasons.DataRepository.Repositories;
using Moq;
using NUnit.Framework;

namespace Futbol.Seasons.Services.Tests.TeamsServiceTests
{
    [TestFixture]
    public class GetYearTeamsStatsTests
    {
        private Mock<ITeamStatsRepository> _repository;
        private IMapper _mapper;
        private Mock<ISeasonConfigService> _seasonService;
        private const short Year = 2020;
        [SetUp]
        public void SetUp()
        {
            _repository = new Mock<ITeamStatsRepository>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new EntitiesMappingProfile());
            });
            _mapper = mockMapper.CreateMapper();
            _seasonService = new Mock<ISeasonConfigService>();
        }

        [Test]
        public async Task Ok_Success()
        {
            _repository.SetupSequence(x => x.GetSeasonTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>())).ReturnsAsync(MockedStats(1).ToList()).ReturnsAsync(MockedStats(2).ToList());
            _seasonService.Setup(x => x.GetConfig(It.IsAny<short>())).ReturnsAsync(MockedConfig());
            var service = new TeamsService(null, _seasonService.Object, null, _repository.Object, null, null,  _mapper);
            var result = await service.GetYearTeamsStatsAsync(Year);

            Assert.NotNull(result);
            Assert.IsInstanceOf<IEnumerable<BusinessEntities.TeamSeasonStats>>(result);
            
            _seasonService.Verify(x => x.GetConfig(It.IsAny<short>()), Times.Once);
            _repository.Verify(x => x.GetSeasonTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()), Times.Exactly(2));
        }

        [Test]
        public void SeasonRepositoryError_ThrowException()
        {
            _seasonService.Setup(x => x.GetConfig(It.IsAny<short>())).ReturnsAsync(MockedConfig());
            _repository.Setup(x => x.GetSeasonTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>())).ThrowsAsync(new DataException());

            var service = new TeamsService(null, _seasonService.Object, null, _repository.Object, null, null,  _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.GetYearTeamsStatsAsync(Year));

            _seasonService.Verify(x => x.GetConfig(It.IsAny<short>()), Times.Once);
            _repository.Verify(x => x.GetSeasonTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()), Times.Once);
        }

        private IEnumerable<DataRepository.DataEntities.TeamSeasonStats> MockedStats(byte season)
        {
            return Enumerable.Range(1,5).Select(tId => new DataRepository.DataEntities.TeamSeasonStats(Year, season, tId)
            {
                TeamName = $"team{tId}", ConferenceId = 0, G = 5,W = (byte)tId, L = (byte)(5-tId), GF = (byte)tId, GA = (byte)tId
            });
        }

        private ChampionshipConfig MockedConfig()
        {
            return new ChampionshipConfig
            {
                Name = "Clausura",
                Year = Year,
                Seasons = Enumerable.Range(1, 2).Select(id => new SeasonConfig
                    { Name = $"S{id}", SeasonId = (byte)id, RoundsCount = 3, Year = Year })
            };
        }
    }
}
