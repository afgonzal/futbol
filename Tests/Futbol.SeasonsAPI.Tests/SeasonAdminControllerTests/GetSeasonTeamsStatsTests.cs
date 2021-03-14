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

namespace Futbol.SeasonsAPI.Tests.SeasonAdminControllerTests
{
    [TestFixture]
    public class GetSeasonTeamsStatsTests
    {
        private Mock<ITeamsService> _service;
        private IMapper _mapper;
        private Mock<ILogger<SeasonController>> _logger;
        private const short Year = 2020;
        private const byte Season = 2;
        [SetUp]
        public void SetUp()
        {
            _service = new Mock<ITeamsService>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ModelMappingProfile());
            });
            _mapper = mockMapper.CreateMapper();
            _logger = new Mock<ILogger<SeasonController>>();
        }

        [Test]
        public async Task Ok_Success()
        {
            _service.Setup(x => x.GetSeasonTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>())).ReturnsAsync(MockedStats());

            var controller = new SeasonController(null, null, _service.Object, _mapper, _logger.Object);
            var result = await controller.GetSeasonTeamsStats(Year, Season);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IActionResult>(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.IsNotNull(((OkObjectResult)result).Value);
            Assert.IsInstanceOf<IEnumerable<TeamStats>>(((OkObjectResult)result).Value);
            Assert.IsTrue(((IEnumerable<TeamStats>)((OkObjectResult)result).Value).Any());

            _service.Verify(x => x.GetSeasonTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()), Times.Once);
            _logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
        }

      
        [Test]
        public async Task ServiceFail_Return500()
        {
            _service.Setup(x => x.GetSeasonTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>())).ThrowsAsync(new DataException());
            var controller = new SeasonController(null, null, _service.Object, _mapper, _logger.Object);
            var result = await controller.GetSeasonTeamsStats(Year, Season);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IActionResult>(result);
            Assert.IsInstanceOf<ObjectResult>(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, ((ObjectResult)result).StatusCode);
            Assert.IsNotNull(((ObjectResult)result).Value);
            Assert.IsNotEmpty(((ObjectResult)result).Value.ToString());

            _service.Verify(x => x.GetSeasonTeamsStatsAsync(It.IsAny<short>(), It.IsAny<byte>()), Times.Once);
            _logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        private IEnumerable<TeamStats> MockedStats()
        {
            return Enumerable.Range(1, 5).Select(id => new TeamStats
                {Id = id, W = (byte) id, G = 5, GF = (byte) (10 + id)});
        }

    }
}
