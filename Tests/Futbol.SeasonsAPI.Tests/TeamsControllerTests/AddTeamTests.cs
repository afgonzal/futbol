using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Castle.Core.Logging;
using Futbol.Seasons.BusinessEntities;
using Futbol.Seasons.Services;
using Futbol.SeasonsAPI.Controllers;
using Futbol.SeasonsAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Futbol.SeasonsAPI.Tests.TeamsControllerTests
{
    [TestFixture]
    public class AddTeamTests
    {
        private Mock<ITeamsService> _service;
        private IMapper _mapper;
        private Mock<ILogger<TeamsController>> _logger;

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<ITeamsService>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ModelMappingProfile());
            });
            _mapper = mockMapper.CreateMapper();
            _logger = new Mock<ILogger<TeamsController>>();
        }

        [Test]
        public async Task Ok_Success()
        {
            var controller = new TeamsController(_service.Object, _mapper, _logger.Object);
            var result = await controller.Add(MockedTeam());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IActionResult>(result);
            Assert.IsInstanceOf<OkResult>(result);

            _service.Verify(x => x.AddTeamAsync(It.IsAny<Team>()), Times.Once);
            _logger.Verify(x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
            _logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task ServiceFail_Return500()
        {
            _service.Setup(x => x.AddTeamAsync(It.IsAny<Team>())).ThrowsAsync(new DataException());
            var controller = new TeamsController(_service.Object, _mapper, _logger.Object);
            var result = await controller.Add(MockedTeam());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IActionResult>(result);
            Assert.IsInstanceOf<ObjectResult>(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, ((ObjectResult)result).StatusCode);
            Assert.IsNotNull(((ObjectResult)result).Value);
            Assert.IsNotEmpty(((ObjectResult)result).Value.ToString());

            _service.Verify(x => x.AddTeamAsync(It.IsAny<Team>()), Times.Once);
            _logger.Verify(x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
            _logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
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
