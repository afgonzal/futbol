﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.DataRepository.Repositories;
using Moq;
using NUnit.Framework;

namespace Futbol.Seasons.Services.Tests.MatchesServiceTests
{
    [TestFixture]
    public class SetRoundResultsTests
    {
        private Mock<IMatchRepository> _repository;
        private IMapper _mapper;
        private const short Year = 2020;
        private const byte Season = 2;
        private const byte Round = 2;
        [SetUp]
        public void SetUp()
        {
            _repository = new Mock<IMatchRepository>();
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new EntitiesMappingProfile());
            });
            _mapper = mockMapper.CreateMapper();
        }

        [Test]
        public async Task Ok_Success()
        {
            _repository.Setup(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedMatches().ToList());

            var service = new MatchesService(_repository.Object, _mapper);
            await service.SetRoundResults(Year, Season,Round, MockedResults().ToList());
            _repository.Verify(x => x.BatchUpsertAsync(It.IsAny<IEnumerable<DataRepository.DataEntities.Match>>()),Times.Once);
            _repository.Verify(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()), Times.Once);
        }

        [Test]
        public void RepositoryMissingMatch_ThrowException()
        {
            _repository.Setup(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedMatches().ToList());

            var wrongMatch = MockedResults().ToList();
            wrongMatch[2].MatchId = 99;

            var service = new MatchesService(_repository.Object, _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.SetRoundResults(Year, Season, Round, wrongMatch));

            _repository.Verify(x => x.BatchUpsertAsync(It.IsAny<IEnumerable<DataRepository.DataEntities.Match>>()), Times.Never);
            _repository.Verify(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()), Times.Once);
        }

        [Test]
        public void RepositoryGetError_ThrowException()
        {
            _repository.Setup(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>())).ThrowsAsync(new DataException());

            var service = new MatchesService(_repository.Object, _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.SetRoundResults(Year, Season, Round, MockedResults().ToList()));

            _repository.Verify(x => x.BatchUpsertAsync(It.IsAny<IEnumerable<DataRepository.DataEntities.Match>>()), Times.Never);
            _repository.Verify(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()), Times.Once);
        }

        [Test]
        public void RepositorySaveError_ThrowException()
        {
            _repository.Setup(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()))
                .ReturnsAsync(MockedMatches().ToList());
            _repository.Setup(x => x.BatchUpsertAsync(It.IsAny<IEnumerable<DataRepository.DataEntities.Match>>())).ThrowsAsync(new DataException());

            var service = new MatchesService(_repository.Object, _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.SetRoundResults(Year, Season, Round, MockedResults().ToList()));

            _repository.Verify(x => x.BatchUpsertAsync(It.IsAny<IEnumerable<DataRepository.DataEntities.Match>>()), Times.Once);
            _repository.Verify(x => x.GetMatchesAsync(It.IsAny<short>(), It.IsAny<byte>(), It.IsAny<byte>()), Times.Once);
        }

        private IEnumerable<BusinessEntities.Match> MockedResults()
        {
            return Enumerable.Range(1, 5).Select(id => new BusinessEntities.Match
            {
               Year = Year,  Season = Season, Round = Round, HomeScore = (byte)id, AwayScore = (byte)(id+2), HomeTeamId = id, AwayTeamId = id+5
            });
        }

        private IEnumerable<DataRepository.DataEntities.Match> MockedMatches()
        {
            return Enumerable.Range(1, 5).Select(id => new DataRepository.DataEntities.Match
            {
                YearSeasonRound = $"{Year}#{Season}#{Round}",
                MatchId = (byte)id,
                WasPlayed = id<4,
                HomeScore = id<4 ? default(byte?): (byte)2,
                AwayScore= id < 4 ? default(byte?) : (byte)1,
                HomeTeamId = id,
                AwayTeamId = id + 5
            });
        }
    }
}
