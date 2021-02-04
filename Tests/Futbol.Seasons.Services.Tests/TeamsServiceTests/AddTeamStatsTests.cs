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
    public class AddTeamStatsTests
    {
        private Mock<ITeamStatsRepository> _repository;
        private IMapper _mapper;

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
            var service = new TeamsService(null, null,_repository.Object, null, null, _mapper);
            var result = await service.AddTeamStatsAsync(13, 2020, 2, "DC");

            Assert.NotNull(result);
            Assert.IsInstanceOf<BusinessEntities.TeamSeasonStats>(result);
            Assert.AreEqual(13, result.Id);
            Assert.AreEqual("DC", result.Name);

            _repository.Verify(x => x.AddAsync(It.IsAny<TeamSeasonStats>()),Times.Once);
        }

        [Test]
        public void RepositoryError_ThrowException()
        {
            _repository.Setup(x => x.AddAsync(It.IsAny<TeamSeasonStats>())).ThrowsAsync(new DataException());

            var service = new TeamsService(null, null,_repository.Object, null, null,  _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.AddTeamStatsAsync(13, 2020, 2, "DC"));

            _repository.Verify(x => x.AddAsync(It.IsAny<TeamSeasonStats>()), Times.Once);
        }

    }
}
