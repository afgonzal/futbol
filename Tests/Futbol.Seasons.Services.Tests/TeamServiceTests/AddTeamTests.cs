using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.DataRepository.DataEntities;
using Futbol.Seasons.DataRepository.Repositories;
using Moq;
using NUnit.Framework;

namespace Futbol.Seasons.Services.Tests.TeamServiceTests
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
            var service = new TeamsService(_repository.Object, _mapper);
            await service.AddTeamAsync(MockedTeam());
            _repository.Verify(x => x.AddAsync(It.IsAny<Team>()),Times.Once);
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
