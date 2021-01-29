using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
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
    public class GetYearTeamsTests
    {
        private Mock<ITeamsService> _service;
        private IMapper _mapper;
        private Mock<ILogger<TeamsControllers>> _logger;
        private const short Year = 2020;
        [SetUp]
        public void SetUp()
        {
            _service = new Mock<ITeamsService>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ModelMappingProfile());
            });
            _mapper = mockMapper.CreateMapper();
            _logger = new Mock<ILogger<TeamsControllers>>();
        }

        [Test]
        public async Task Ok_Success()
        {
            _service.Setup(x => x.GetYearTeamsAsync(It.IsAny<short>())).ReturnsAsync(MockedTeams(Year));

            var controller = new TeamsControllers(_service.Object, _mapper, _logger.Object);
            var result = await controller.GetYearTeams(Year);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IActionResult>(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.IsNotNull(((OkObjectResult)result).Value);
            Assert.IsInstanceOf<IEnumerable<TeamModel>>(((OkObjectResult)result).Value);
            Assert.IsTrue(((IEnumerable<TeamModel>)((OkObjectResult)result).Value).Any());

            _service.Verify(x => x.GetYearTeamsAsync(It.IsAny<short>()), Times.Once);
            _logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task ServiceFail_Return500()
        {
            _service.Setup(x => x.GetYearTeamsAsync(It.IsAny<short>())).ThrowsAsync(new DataException());
            var controller = new TeamsControllers(_service.Object, _mapper, _logger.Object);
            var result = await controller.GetYearTeams(Year);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IActionResult>(result);
            Assert.IsInstanceOf<ObjectResult>(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, ((ObjectResult)result).StatusCode);
            Assert.IsNotNull(((ObjectResult)result).Value);
            Assert.IsNotEmpty(((ObjectResult)result).Value.ToString());

            _service.Verify(x => x.GetYearTeamsAsync(It.IsAny<short>()), Times.Once);
            _logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        private IEnumerable<Team> MockedTeams(short year)
        {
            return Enumerable.Range(1, 5).Select(tId => new Team
            {
                Year = year,
                Id = tId,
                Name = $"team{tId}",
                ConferenceId = 0,
                Delegates = new List<string> { "Favio", "Ale" },
                Years = new List<short> { 2020, 2021 }
            });
        }
    }
}
