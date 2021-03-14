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
    public class GetHistoricTeamsStatsTests
    {
        private Mock<ITeamStatsRepository> _repository;
        private IMapper _mapper;
        private Mock<ISeasonConfigService> _seasonService;
        private readonly short[] Years = new short[] {2018,2019};
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
            _repository.SetupSequence(x => x.GetSeasonTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>())).ReturnsAsync(MockedStats(Years[0],1).ToList()).ReturnsAsync(MockedStats(Years[0],2).ToList());
            _seasonService.Setup(x => x.GetConfig(It.IsAny<short>())).ReturnsAsync(MockedConfig(Years[0]));
            var service = new TeamsService(null, _seasonService.Object, null, _repository.Object, null, null, null, _mapper);
            var result = await service.GetHistoricTeamsStatsAsync(Years);

            Assert.NotNull(result);
            Assert.IsInstanceOf<IEnumerable<BusinessEntities.TeamStats>>(result);
            Assert.True(result.Any());
            Assert.AreEqual(5, result.Count());
            Assert.AreEqual(6, result.First().Pts);
            Assert.AreEqual(30, result.Last().Pts);
            Assert.AreEqual(10, result.First().G);

            _seasonService.Verify(x => x.GetConfig(It.IsAny<short>()), Times.Exactly(2));
            _repository.Verify(x => x.GetSeasonTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()), Times.Exactly(4));
        }

        [Test]
        public void SeasonRepositoryError_ThrowException()
        {
            _seasonService.Setup(x => x.GetConfig(It.IsAny<short>())).ReturnsAsync(MockedConfig(Years[0]));
            _repository.Setup(x => x.GetSeasonTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>())).ThrowsAsync(new DataException());

            var service = new TeamsService(null, _seasonService.Object, null, _repository.Object, null, null, null, _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.GetHistoricTeamsStatsAsync(Years));

            _seasonService.Verify(x => x.GetConfig(It.IsAny<short>()), Times.Once);
            _repository.Verify(x => x.GetSeasonTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()), Times.Once);
        }

        private IEnumerable<DataRepository.DataEntities.TeamSeasonStats> MockedStats(short year, byte season)
        {
            return Enumerable.Range(1,5).Select(tId => new DataRepository.DataEntities.TeamSeasonStats(year, season, tId)
            {
                TeamName = $"team{tId}", G = 5,W = (byte)tId, L = (byte)(5-tId), GF = (byte)tId, GA = (byte)tId
            });
        }

        private ChampionshipConfig MockedConfig(short year)
        {
            return new ChampionshipConfig
            {
                Name = "Clausura",
                Year = year,
                Seasons = Enumerable.Range(1, 2).Select(id => new SeasonConfig
                    { Name = $"S{id}", SeasonId = (byte)id, RoundsCount = 3, Year = year })
            };
        }
    }
}
