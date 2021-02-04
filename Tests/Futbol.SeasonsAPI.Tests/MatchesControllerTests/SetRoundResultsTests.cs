using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

namespace Futbol.SeasonsAPI.Tests.MatchesControllerTests
{
    [TestFixture]
    public class SetRoundResultsTests
    {
        private Mock<IMatchesService> _service;
        private IMapper _mapper;
        private Mock<ILogger<MatchesController>> _logger;
        private Mock<ITeamsService> _teamsService;
        private const short Year = 2020;
        private const byte Season = 1;
        private const byte Round = 2;
        [SetUp]
        public void SetUp()
        {
            _service = new Mock<IMatchesService>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ModelMappingProfile());
            });
            _mapper = mockMapper.CreateMapper();
            _logger = new Mock<ILogger<MatchesController>>();
            _teamsService = new Mock<ITeamsService>();
        }

        [Test]
        public async Task Ok_Success()
        {
            _teamsService.Setup(x => x.GetYearTeamsAsync(It.IsAny<short>())).ReturnsAsync(MockedTeams());
            var controller = new MatchesController(_service.Object, _teamsService.Object, _mapper, _logger.Object);
            var result = await controller.SetRoundResults(Year, Season,Round, MockedMatches().ToList());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IActionResult>(result);
            Assert.IsInstanceOf<OkResult>(result);

            _service.Verify(x => x.SetRoundResults(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>(), It.IsAny<IList<Seasons.BusinessEntities.Match >>()), Times.Once);
            _logger.Verify(x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
            _logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
            _teamsService.Verify(x => x.GetYearTeamsAsync(It.IsAny<short>()), Times.Once);
        }

        [Test]
        public async Task WrongTeam_Return500()
        {
            var wrongTeams = MockedMatches().ToList();
            wrongTeams[0].HomeTeamAbbr = "wrong";
            _teamsService.Setup(x => x.GetYearTeamsAsync(It.IsAny<short>())).ReturnsAsync(MockedTeams());
            var controller = new MatchesController(_service.Object, _teamsService.Object, _mapper, _logger.Object);
            var result = await controller.SetRoundResults(Year, Season,Round, wrongTeams);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IActionResult>(result);
            Assert.IsInstanceOf<ObjectResult>(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, ((ObjectResult)result).StatusCode);
            Assert.IsNotNull(((ObjectResult)result).Value);
            Assert.IsNotEmpty(((ObjectResult)result).Value.ToString());

            _service.Verify(x => x.SetRoundResults(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>(), It.IsAny<IList<Seasons.BusinessEntities.Match>>()), Times.Never);
            _logger.Verify(x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
            _logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
            _teamsService.Verify(x => x.GetYearTeamsAsync(It.IsAny<short>()), Times.Once);
        }

        [Test]
        public async Task GetTeamsFails_Return500()
        {
            _teamsService.Setup(x => x.GetYearTeamsAsync(It.IsAny<short>())).ThrowsAsync(new DataException());
            var controller = new MatchesController(_service.Object, _teamsService.Object, _mapper, _logger.Object);
            var result = await controller.SetRoundResults(Year, Season, Round,MockedMatches().ToList());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IActionResult>(result);
            Assert.IsInstanceOf<ObjectResult>(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, ((ObjectResult)result).StatusCode);
            Assert.IsNotNull(((ObjectResult)result).Value);
            Assert.IsNotEmpty(((ObjectResult)result).Value.ToString());

            _service.Verify(x => x.SetRoundResults(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>(),It.IsAny<IList<Seasons.BusinessEntities.Match>>()), Times.Never);
            _logger.Verify(x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
            _logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
            _teamsService.Verify(x => x.GetYearTeamsAsync(It.IsAny<short>()), Times.Once);
        }

        [Test]
        public async Task ServiceFail_Return500()
        {
            _teamsService.Setup(x => x.GetYearTeamsAsync(It.IsAny<short>())).ReturnsAsync(MockedTeams());
            _service.Setup(x => x.SetRoundResults(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>(), It.IsAny<IList<Seasons.BusinessEntities.Match>>())).ThrowsAsync(new DataException());
            var controller = new MatchesController(_service.Object, _teamsService.Object, _mapper, _logger.Object);
            var result = await controller.SetRoundResults(Year, Season,Round, MockedMatches().ToList());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IActionResult>(result);
            Assert.IsInstanceOf<ObjectResult>(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, ((ObjectResult)result).StatusCode);
            Assert.IsNotNull(((ObjectResult)result).Value);
            Assert.IsNotEmpty(((ObjectResult)result).Value.ToString());

            _service.Verify(x => x.SetRoundResults(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>(), It.IsAny<IList<Seasons.BusinessEntities.Match>>()), Times.Once);
            _logger.Verify(x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
            _logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
            _teamsService.Verify(x => x.GetYearTeamsAsync(It.IsAny<short>()), Times.Once);
        }

        private IEnumerable<MatchResultRequest> MockedMatches()
        {
            return Enumerable.Range(1, 5).Select(tid => new MatchResultRequest
            {
                    HomeTeamAbbr = $"team{tid}",
                    AwayTeamAbbr = $"team{tid + 5}",
                    HomeScore = (byte)tid,
                    AwayScore = (byte)(tid+2)
                });
        }

        private IEnumerable<Team> MockedTeams()
        {
            return Enumerable.Range(1, 10).Select(tid => new Team
            {
                Id = tid,
                Name = $"team{tid}",
                ConferenceId = 0,
                Delegates = new string[] { "Favio", "Ale" },
                Year = 2020,
                Years = new short[] { 2020, 2021 },
                Abbreviation = $"team{tid}"
            });
        }
    }
}
