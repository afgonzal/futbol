using System.Collections.Generic;
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
    public class AddTeamTests
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
            var service = new TeamsService(_repository.Object, null, _mapper);
            await service.AddTeamAsync(MockedTeam());
            _repository.Verify(x => x.AddAsync(It.IsAny<TeamProfile>()),Times.Once);
        }

        [Test]
        public void RepositoryError_ThrowException()
        {
            _repository.Setup(x => x.AddAsync(It.IsAny<TeamProfile>())).ThrowsAsync(new DataException());

            var service = new TeamsService(_repository.Object, null, _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.AddTeamAsync(MockedTeam()));

            _repository.Verify(x => x.AddAsync(It.IsAny<TeamProfile>()), Times.Once);
        }

        private BusinessEntities.Team MockedTeam()
        {
            return new BusinessEntities.Team
            {
                Id = 3, Name = "DC United", ConferenceId = 0, Delegates = new string[] {"Favio", "Ale"}, Year = 2020,
                Years = new short[] {2020, 2021}
            };
        }
    }
}
