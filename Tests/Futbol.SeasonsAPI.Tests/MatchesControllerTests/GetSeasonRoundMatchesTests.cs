using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
    public class GetSeasonRoundMatchesTests
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
            _service.Setup(x => x.GetSeasonRoundMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedMatches());
            var controller = new MatchesController(_service.Object, _teamsService.Object, _mapper, _logger.Object);
            var result = await controller.GetSeasonRoundMatchesAsync(Year, Season, Round);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IActionResult>(result);
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.NotNull(((OkObjectResult)result).Value);
            Assert.IsInstanceOf<IEnumerable<MatchModel>>(((OkObjectResult)result).Value);
            Assert.True(((IEnumerable<MatchModel>)((OkObjectResult)result).Value).Any());



            _service.Verify(x => x.GetSeasonRoundMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()), Times.Once);
            _logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
        }
      
        [Test]
        public async Task ServiceFail_Return500()
        {
            _service.Setup(x => x.GetSeasonRoundMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>())).ThrowsAsync(new DataException());
            var controller = new MatchesController(_service.Object, _teamsService.Object, _mapper, _logger.Object);
            var result = await controller.GetSeasonRoundMatchesAsync(Year, Season, Round);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IActionResult>(result);
            Assert.IsInstanceOf<ObjectResult>(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, ((ObjectResult)result).StatusCode);
            Assert.IsNotNull(((ObjectResult)result).Value);
            Assert.IsNotEmpty(((ObjectResult)result).Value.ToString());

            _service.Verify(x => x.GetSeasonRoundMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()), Times.Once);
            _logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        private IEnumerable<Seasons.BusinessEntities.Match> MockedMatches()
        {
            return Enumerable.Range(1, 5).Select(tid => new Seasons.BusinessEntities.Match
            {
                  HomeTeamId = tid, AwayTeamId =  tid+5, MatchId = (byte)tid, HomeTeamName = $"team{tid}", AwayTeamName = $"team{tid+5}"

            });
        }

      
    }
}
