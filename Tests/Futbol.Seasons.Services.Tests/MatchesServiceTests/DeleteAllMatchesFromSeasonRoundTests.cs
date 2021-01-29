using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.DataRepository.Repositories;
using Moq;
using NUnit.Framework;

namespace Futbol.Seasons.Services.Tests.MatchesServiceTests
{
    [TestFixture]
    public class DeleteAllMatchesFromSeasonRoundTests
    {
        private Mock<IMatchRepository> _repository;
        private IMapper _mapper;
        private const short Year = 2020;
        private const byte Season = 0;
        private const byte Round = 0;

        [SetUp]
        public void SetUp()
        {
            _repository = new Mock<IMatchRepository>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new EntitiesMappingProfile());
            });
            _mapper = mockMapper.CreateMapper();
        }

        [Test]
        public async Task Ok_Success()
        {
            _repository.Setup(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedMatches().ToList());

            var service = new MatchService(_repository.Object, _mapper);
            await service.DeleteAllMatchesFromSeasonRoundAsync(Year, Season, Round);
            _repository.Verify(x => x.DeleteMatchesAsync(It.IsAny<IEnumerable<DataRepository.DataEntities.Match>>()),Times.Once);
            _repository.Verify(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()),
                Times.Once);
        }

        [Test]
        public void RepositoryError_ThrowException()
        {
            _repository.Setup(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedMatches().ToList());
            _repository.Setup(x => x.DeleteMatchesAsync(It.IsAny<IEnumerable< DataRepository.DataEntities.Match>> ())).ThrowsAsync(new DataException());

            var service = new MatchService(_repository.Object, _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.DeleteAllMatchesFromSeasonRoundAsync(Year, Season, Round));

            _repository.Verify(x => x.DeleteMatchesAsync(It.IsAny<IEnumerable< DataRepository.DataEntities.Match>> ()), Times.Once);
            _repository.Verify(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()),
                Times.Once);
        }


        [Test]
        public void GetMatchesError_ThrowException()
        {
            _repository.Setup(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>())).ThrowsAsync(new DataException());
            
            var service = new MatchService(_repository.Object, _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.DeleteAllMatchesFromSeasonRoundAsync(Year, Season, Round));

            _repository.Verify(x => x.DeleteMatchesAsync(It.IsAny<IEnumerable<DataRepository.DataEntities.Match>>()), Times.Never);
        }

        private IEnumerable<DataRepository.DataEntities.Match> MockedMatches()
        {
            return Enumerable.Range(1, 5).Select(id => new DataRepository.DataEntities.Match
            {
                YearSeasonRound = $"{Year}#{Season}#{Round}",
                MatchId = id
            });
        }
    }
}
