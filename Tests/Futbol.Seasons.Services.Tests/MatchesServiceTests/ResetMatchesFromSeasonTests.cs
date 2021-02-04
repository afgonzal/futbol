using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.BusinessEntities;
using Futbol.Seasons.DataRepository.Repositories;
using Moq;
using NUnit.Framework;

namespace Futbol.Seasons.Services.Tests.MatchesServiceTests
{
    [TestFixture]
    public class ResetMatchesFromSeasonTests
    {
        private Mock<IMatchRepository> _repository;
        private IMapper _mapper;
        private Mock<ISeasonConfigService> _configService;
        private const short Year = 2020;
        private const byte Season = 2;
        [SetUp]
        public void SetUp()
        {
            _repository = new Mock<IMatchRepository>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new EntitiesMappingProfile());
            });
            _mapper = mockMapper.CreateMapper();
            _configService = new Mock<ISeasonConfigService>();
            _configService.Setup(x => x.GetConfig(It.IsAny<short>(), It.IsAny<byte>()))
                .ReturnsAsync(new SeasonConfig { RoundsCount = 10 });
        }

        [Test]
        public async Task Ok_Success()
        {
            _repository.Setup(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>())).ReturnsAsync(MockedMatches().ToList());
            
            var service = new MatchesService(_repository.Object, _configService.Object, _mapper);
            await service.ResetAllMatchesFromSeasonAsync(Year, Season);


            _repository.Verify(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()), Times.Exactly(10));
            _configService.Verify(x => x.GetConfig(It.IsAny<short>(), It.IsAny<byte>()), Times.Once);
            _repository.Verify(x => x.BatchUpsertAsync(It.IsAny<IEnumerable<DataRepository.DataEntities.Match>>()), Times.Exactly(10));
        }

        [Test]
        public void GetMatchesError_ThrowException()
        {
            _repository.Setup(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>())).ThrowsAsync(new DataException());

            var service = new MatchesService(_repository.Object, _configService.Object, _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.ResetAllMatchesFromSeasonAsync(Year, Season));

            _repository.Verify(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()), Times.Once);
            _configService.Verify(x => x.GetConfig(It.IsAny<short>(), It.IsAny<byte>()), Times.Once);
            _repository.Verify(x => x.BatchUpsertAsync(It.IsAny<IEnumerable<DataRepository.DataEntities.Match>>()), Times.Never);
        }

        [Test]
        public void RepositoryFails_ThrowException()
        {
            _repository.Setup(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>())).ReturnsAsync(MockedMatches().ToList());
            _repository.Setup(x => x.BatchUpsertAsync(It.IsAny<IEnumerable<DataRepository.DataEntities.Match>>())).ThrowsAsync(new DataException());

            var service = new MatchesService(_repository.Object, _configService.Object, _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.ResetAllMatchesFromSeasonAsync(Year, Season));

            _repository.Verify(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()), Times.Once);
            _configService.Verify(x => x.GetConfig(It.IsAny<short>(), It.IsAny<byte>()), Times.Once);
            _repository.Verify(x => x.BatchUpsertAsync(It.IsAny<IEnumerable<DataRepository.DataEntities.Match>>()), Times.Once);
        }


        [Test]
        public void NoConfig_ThrowException()
        {
            _configService.Setup(x => x.GetConfig(It.IsAny<short>(), It.IsAny<byte>()))
                .ReturnsAsync((SeasonConfig) null);

            var service = new MatchesService(_repository.Object, _configService.Object, _mapper);
            Assert.ThrowsAsync<ArgumentException>(async () => await service.ResetAllMatchesFromSeasonAsync(Year, Season));

            _repository.Verify(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()), Times.Never);
            _configService.Verify(x => x.GetConfig(It.IsAny<short>(), It.IsAny<byte>()), Times.Once);
            _repository.Verify(x => x.BatchUpsertAsync(It.IsAny<IEnumerable<DataRepository.DataEntities.Match>>()), Times.Never);
        }

        private IEnumerable<DataRepository.DataEntities.Match> MockedMatches()
        {
            return Enumerable.Range(1, 5).Select(id => new DataRepository.DataEntities.Match
            {
                YearSeasonRound = $"{Year}#{Season}#{3}",
                MatchId = (byte)id
            });
        }
    }
}
