using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.DataRepository.Repositories;
using Moq;
using NUnit.Framework;

namespace Futbol.Seasons.Services.Tests.TeamsServiceTests
{
    [TestFixture]
    public class GetTeamSeasonMatchesTests
    {
        private Mock<IMatchRepository> _repository;
        private IMapper _mapper;
        private const short Year = 2020;
        private const byte Season = 0;
        private const int TeamId = 3;
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
            _repository.Setup(x => x.GetTeamSeasonMatches(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<int>())).ReturnsAsync(MockedMatches().ToList());
            var service = new TeamsService(null, null, null,null,_repository.Object, null, null, _mapper);
            var result = await service.GetTeamSeasonMatchesAsync(TeamId, Year, Season);

            Assert.NotNull(result);
            Assert.IsInstanceOf<IEnumerable<BusinessEntities.Match>>(result);

            _repository.Verify(x => x.GetTeamSeasonMatches(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void RepositoryError_ThrowException()
        {
            _repository.Setup(x => x.GetTeamSeasonMatches(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<int>())).ThrowsAsync(new DataException());

            var service = new TeamsService(null, null, null,null,_repository.Object, null, null, _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.GetTeamSeasonMatchesAsync(TeamId, Year, Season));

            _repository.Verify(x => x.GetTeamSeasonMatches(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<int>()), Times.Once);
        }

        private IEnumerable<DataRepository.DataEntities.Match> MockedMatches()
        {
            return Enumerable.Range(1, 5).Select(id => new DataRepository.DataEntities.Match
            {
                YearSeasonRound = $"{Year}#{Season}#{id}",
                MatchId = (byte)id
            });
        }
    }
}
