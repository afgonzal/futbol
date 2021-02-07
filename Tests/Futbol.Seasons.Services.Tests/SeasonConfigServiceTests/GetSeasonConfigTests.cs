using System.Data;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.BusinessEntities;
using Futbol.Seasons.DataRepository.Repositories;
using Moq;
using NUnit.Framework;

namespace Futbol.Seasons.Services.Tests.SeasonConfigServiceTests
{
    [TestFixture]
    public class GetSeasonConfigTests
    {
        private Mock<ISeasonConfigRepository> _repository;
        private IMapper _mapper;
        private const short Year = 2020;
        private const byte Season = 2;
        [SetUp]
        public void SetUp()
        {
            _repository = new Mock<ISeasonConfigRepository>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new EntitiesMappingProfile());
            });
            _mapper = mockMapper.CreateMapper();
        }

        [Test]
        public async Task Ok_Success()
        {
            _repository.Setup(x => x.GetByKeyAsync(It.IsAny<short>(), It.IsAny<string>())).ReturnsAsync(MockedConfig());

            var service = new SeasonConfigService(null, _repository.Object, _mapper);
            var result = await service.GetConfig(Year, Season);

            Assert.NotNull(result);
            Assert.IsInstanceOf<SeasonConfig>(result);
            Assert.NotNull(result.Name);
            Assert.AreEqual(13, result.RoundsCount);

            _repository.Verify(x => x.GetByKeyAsync(It.IsAny<short>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task DbError_ThrowException()
        {
            _repository.Setup(x => x.GetByKeyAsync(It.IsAny<short>(), It.IsAny<string>()))
                .ThrowsAsync(new DataException());

            var service = new SeasonConfigService(null, _repository.Object, _mapper);
            Assert.ThrowsAsync<DataException>(async () =>await service.GetConfig(Year, Season));

            _repository.Verify(x => x.GetByKeyAsync(It.IsAny<short>(), It.IsAny<string>()), Times.Once);
        }

        private DataRepository.DataEntities.SeasonConfig MockedConfig()
        {
            return new DataRepository.DataEntities.SeasonConfig { Name = "Clausura", SeasonId = Season, Year = Year, RoundsCount = 13};
        }
    }
}
