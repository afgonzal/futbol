using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.BusinessEntities;
using Futbol.Seasons.DataRepository.Repositories;
using Moq;
using NUnit.Framework;

namespace Futbol.Seasons.Services.Tests.SeasonConfigServiceTests
{
    [TestFixture]
    public class GetChampionshipConfigTests
    {
        private Mock<IChampionshipConfigRepository> _repository;
        private IMapper _mapper;
        private const short Year = 2020;
        [SetUp]
        public void SetUp()
        {
            _repository = new Mock<IChampionshipConfigRepository>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new EntitiesMappingProfile());
            });
            _mapper = mockMapper.CreateMapper();
        }

        [Test]
        public async Task Ok_Success()
        {
            _repository.Setup(x => x.GetYearConfig(It.IsAny<short>())).ReturnsAsync(MockedConfig());

            var service = new SeasonConfigService(_repository.Object,null, _mapper);
            var result = await service.GetConfig(Year);

            Assert.NotNull(result);
            Assert.IsInstanceOf<ChampionshipConfig>(result);
            Assert.NotNull(result.Name);
            Assert.True(result.Seasons.Any());

            _repository.Verify(x => x.GetYearConfig(It.IsAny<short>()), Times.Once);
        }

        [Test]
        public void DbError_ThrowException()
        {
            _repository.Setup(x => x.GetYearConfig(It.IsAny<short>()))
                .ThrowsAsync(new DataException());

            var service = new SeasonConfigService(_repository.Object, null,_mapper);
            Assert.ThrowsAsync<DataException>(async () =>await service.GetConfig(Year));

            _repository.Verify(x => x.GetYearConfig(It.IsAny<short>()), Times.Once);
        }

        private DataRepository.DataEntities.ChampionshipConfig MockedConfig()
        {
            return new DataRepository.DataEntities.ChampionshipConfig
            {
                Name = "Clausura", SeasonsIds = new byte[] {1, 2}, Year = Year,
                Seasons = Enumerable.Range(1, 2).Select(id => new DataRepository.DataEntities.SeasonConfig
                    {Name = $"S{id}", SeasonId = (byte)id, RoundsCount = 3, Year = Year})
            };
        }
    }
}
