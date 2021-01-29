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
    public class BulkAddTeamsTests
    {
        private Mock<ITeamRepository> _repository;
        private IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            _repository = new Mock<ITeamRepository>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new EntitiesMappingProfile());
            });
            _mapper = mockMapper.CreateMapper();
        }

        [Test]
        public async Task Ok_Success()
        {
            var service = new TeamsService(_repository.Object, _mapper);
            await service.BulkAddTeams(MockedTeams());
            _repository.Verify(x => x.BatchAddAsync(It.IsAny<IEnumerable<Team>>()),Times.Once);
        }

        [Test]
        public void RepositoryError_ThrowException()
        {
            _repository.Setup(x => x.BatchAddAsync(It.IsAny<IEnumerable<Team>>())).ThrowsAsync(new DataException());

            var service = new TeamsService(_repository.Object, _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.BulkAddTeams(MockedTeams()));

            _repository.Verify(x => x.BatchAddAsync(It.IsAny<IEnumerable<Team>>()), Times.Once);
        }

        private IEnumerable<BusinessEntities.Team> MockedTeams()
        {
            return Enumerable.Range(1, 5).Select(tid => new BusinessEntities.Team
            {
                Id = tid, Name = $"team{tid}", ConferenceId = 0, Delegates = new string[] {"Favio", "Ale"}, Year = 2020,
                Years = new short[] {2020, 2021}
            });
        }
    }
}
