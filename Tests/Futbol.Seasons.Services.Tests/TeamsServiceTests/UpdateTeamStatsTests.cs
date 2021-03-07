using System.Data;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.BusinessEntities;
using Futbol.Seasons.DataRepository.Repositories;
using Moq;
using NUnit.Framework;

namespace Futbol.Seasons.Services.Tests.TeamsServiceTests
{
    [TestFixture]
    public class UpdateTeamStatsTests
    {
        private Mock<ITeamStatsRepository> _repository;
        private IMapper _mapper;
        private const short Year = 2020;
        private const byte Season = 2;
        private const int TeamId = 35;
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
            var service = new TeamsService(null, null, null,_repository.Object, null, null, _mapper);
            await service.UpdateTeamStatsAsync(TeamId, Year, Season, MockStats());


            _repository.Verify(x => x.UpdateAsync(It.IsAny<DataRepository.DataEntities.TeamSeasonStats>()),Times.Once);
        }

      

        [Test]
        public void RepositoryError_ThrowException()
        {
            _repository.Setup(x => x.UpdateAsync(It.IsAny<DataRepository.DataEntities.TeamSeasonStats>())).ThrowsAsync(new DataException());

            var service = new TeamsService(null, null, null,_repository.Object, null, null,  _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.UpdateTeamStatsAsync(TeamId, Year, Season, MockStats() ));

            _repository.Verify(x => x.UpdateAsync(It.IsAny<DataRepository.DataEntities.TeamSeasonStats>()), Times.Once);
        }
        private TeamStats MockStats()
        {
            return new TeamStats {G = 3, W = 2, L = 1, GA = 2, GF = 15};
        }
    }
}
