using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.DataRepository.DataEntities;
using Futbol.Seasons.DataRepository.Repositories;
using Moq;
using NUnit.Framework;

namespace Futbol.Seasons.Services.Tests.TeamServiceTests
{
    [TestFixture]
    public class DeleteAllTeamsFromSeason
    {
        private Mock<ITeamRepository> _repository;
        private IMapper _mapper;
        private const short Year = 2020;

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
            await service.DeleteAllTeamsFromSeasonAsync(Year);
            _repository.Verify(x => x.DeleteTeamsAsync(It.IsAny<IEnumerable<Team>>()),Times.Once);
        }

        [Test]
        public void RepositoryError_ThrowException()
        {
            _repository.Setup(x => x.DeleteTeamsAsync(It.IsAny<IEnumerable<Team>>())).ThrowsAsync(new DataException());

            var service = new TeamsService(_repository.Object, _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.DeleteAllTeamsFromSeasonAsync(Year));

            _repository.Verify(x => x.DeleteTeamsAsync(It.IsAny<IEnumerable<Team>>()), Times.Once);
        }
    }
}
