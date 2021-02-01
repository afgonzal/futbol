using System;
using System.Data;
using System.Threading.Tasks;
using Futbol.Seasons.Services;
using Futbol.SeasonsAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Futbol.SeasonsAPI.Tests.SeasonAdminControllerTests
{
    [TestFixture]
    public class CleanSeasonTests
    {
        private Mock<ITeamsService> _teamsService;
        private Mock<ILogger<TeamsController>> _logger;
        private Mock<IMatchesService> _matchesService;
        private const short Year = 2020;
        [SetUp]
        public void SetUp()
        {
            _teamsService = new Mock<ITeamsService>();
            _logger = new Mock<ILogger<TeamsController>>();
            _matchesService = new Mock<IMatchesService>();
        }

        [Test]
        public async Task Ok_Success()
        {
            var controller = new SeasonAdminControllers(_teamsService.Object, _matchesService.Object, _logger.Object);
            var result = await controller.DeleteSeason(Year);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IActionResult>(result);
            Assert.IsInstanceOf<OkResult>(result);

            _teamsService.Verify(x => x.DeleteAllTeamsFromSeasonAsync(It.IsAny<short>()), Times.Once);
            _matchesService.Verify(x => x.DeleteAllMatchesFromSeasonRoundAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()), Times.Exactly(4));
            _logger.Verify(x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(5));
            _logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task ServiceFail_Return500()
        {
            _teamsService.Setup(x => x.DeleteAllTeamsFromSeasonAsync(It.IsAny<short>())).ThrowsAsync(new DataException());
            var controller = new SeasonAdminControllers(_teamsService.Object, _matchesService.Object, _logger.Object);
            var result = await controller.DeleteSeason(Year);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IActionResult>(result);
            Assert.IsInstanceOf<ObjectResult>(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, ((ObjectResult)result).StatusCode);
            Assert.IsNotNull(((ObjectResult)result).Value);
            Assert.IsNotEmpty(((ObjectResult)result).Value.ToString());

            _teamsService.Verify(x => x.DeleteAllTeamsFromSeasonAsync(It.IsAny<short>()), Times.Once);
            _logger.Verify(x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
            _logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
            _matchesService.Verify(x => x.DeleteAllMatchesFromSeasonRoundAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()), Times.Never);
        }
    }
}
