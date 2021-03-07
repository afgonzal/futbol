using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.DataRepository.DataEntities;
using Futbol.Seasons.DataRepository.Repositories;
using Moq;
using NUnit.Framework;

namespace Futbol.Seasons.Services.Tests.TeamsServiceTests
{
    [TestFixture]
    public class BulkAddTeamsStatsTests
    {
        private Mock<ITeamStatsRepository> _repository;
        private IMapper _mapper;
        private const short Year = 2020;
        private const byte Season = 2;
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
            var service = new TeamsService(null, null, null,_repository.Object, null, null,  _mapper);
            await service.BulkUpsertTeamStats(Year, Season,MockedStats());
            _repository.Verify(x => x.BatchUpsertAsync(It.IsAny<IEnumerable<TeamSeasonStats>>()),Times.Once);
        }

        [Test]
        public void RepositoryError_ThrowException()
        {
            _repository.Setup(x => x.BatchUpsertAsync(It.IsAny<IEnumerable<TeamSeasonStats>>())).ThrowsAsync(new DataException());

            var service = new TeamsService(null, null, null,_repository.Object, null, null,  _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.BulkUpsertTeamStats(Year, Season,MockedStats()));

            _repository.Verify(x => x.BatchUpsertAsync(It.IsAny<IEnumerable<TeamSeasonStats>>()), Times.Once);
        }

        private IEnumerable<BusinessEntities.TeamStats> MockedStats()
        {
            return Enumerable.Range(1, 5).Select(tid => new BusinessEntities.TeamStats
            {
                Id = tid, Name = $"team{tid}", W=(byte)tid, GF=(byte)tid , GA = (byte)(20-tid), G = (byte)5
            });
        }
    }
}
