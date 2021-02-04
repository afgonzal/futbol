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
            _repository.Setup(x => x.GetYearTeamsAsync(It.IsAny<short>())).ReturnsAsync(MockedTeams().ToList());

            var service = new TeamsService(null, _repository.Object, null, null, null, _mapper);
            await service.DeleteAllTeamsFromSeasonAsync(Year);
            _repository.Verify(x => x.BatchDeleteAsync(It.IsAny<IEnumerable<TeamProfile>>()),Times.Once);
            _repository.Verify(x => x.GetYearTeamsAsync(It.IsAny<short>()), Times.Once);
        }

        [Test]
        public void RepositoryError_ThrowException()
        {
            _repository.Setup(x => x.GetYearTeamsAsync(It.IsAny<short>())).ReturnsAsync(MockedTeams().ToList());
            _repository.Setup(x => x.BatchDeleteAsync(It.IsAny<IEnumerable<TeamProfile>>())).ThrowsAsync(new DataException());

            var service = new TeamsService(null, _repository.Object, null, null, null, _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.DeleteAllTeamsFromSeasonAsync(Year));

            _repository.Verify(x => x.BatchDeleteAsync(It.IsAny<IEnumerable<TeamProfile>>()), Times.Once);
            _repository.Verify(x => x.GetYearTeamsAsync(It.IsAny<short>()), Times.Once);
        }

        [Test]
        public void GetTeamsError_ThrowException()
        {
            _repository.Setup(x => x.GetYearTeamsAsync(It.IsAny<short>())).ThrowsAsync(new DataException());

            var service = new TeamsService(null, _repository.Object, null, null, null, _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.DeleteAllTeamsFromSeasonAsync(Year));

            _repository.Verify(x => x.BatchDeleteAsync(It.IsAny<IEnumerable<TeamProfile>>()), Times.Never);
            _repository.Verify(x => x.GetYearTeamsAsync(It.IsAny<short>()), Times.Once);
        }

        private IEnumerable<TeamProfile> MockedTeams()
        {
            return Enumerable.Range(1, 5).Select(tId => new TeamProfile
            {
                Year = Year,
                TeamId = tId,
                TeamName = $"team{tId}",
                ConferenceId = 0,
                Delegates = new List<string> { "Favio", "Ale" },
                Years = new List<short> { 2020, 2021 }
            });
        }
    }
}
