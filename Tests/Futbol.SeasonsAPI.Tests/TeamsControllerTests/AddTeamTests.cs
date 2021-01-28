using System;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.BusinessEntities;
using Futbol.Seasons.Services;
using Futbol.SeasonsAPI.Controllers;
using Futbol.SeasonsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Futbol.SeasonsAPI.Tests.TeamsControllerTests
{
    [TestFixture]
    public class AddTeamTests
    {
        private Mock<ITeamsService> _service;
        private IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<ITeamsService>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ModelMappingProfile());
            });
            _mapper = mockMapper.CreateMapper();
        }

        [Test]
        public async Task Ok_Success()
        {
            var controller = new TeamsControllers(_service.Object, _mapper);
            var result = await controller.Add(MockedTeam());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IActionResult>(result);
            Assert.IsInstanceOf<OkResult>(result);

            _service.Verify(x => x.AddTeamAsync(It.IsAny<Team>()), Times.Once);
        }

        public TeamAddRequest MockedTeam()
        {
            return new TeamAddRequest
            {
                Year = 2020,
                Name = "DC United",
                ConferenceId = 0,
                Years = new short[] {2020, 2021},
                Delegates = new string[] {"Favio", "Ale"}
            };
        }
    }
}
