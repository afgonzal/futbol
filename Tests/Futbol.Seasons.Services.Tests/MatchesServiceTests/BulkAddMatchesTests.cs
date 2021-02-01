using System.Collections.Generic;
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
    public class BulkAddMatchesTests
    {
        private Mock<IMatchRepository> _repository;
        private IMapper _mapper;

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
            var service = new MatchesService(_repository.Object, _mapper);
            await service.BulkAddMatches(MockedMatches().ToList());
            _repository.Verify(x => x.BatchUpsertAsync(It.IsAny<IEnumerable<DataRepository.DataEntities.Match>>()),Times.Once);
        }

        [Test]
        public void RepositoryError_ThrowException()
        {
            _repository.Setup(x => x.BatchUpsertAsync(It.IsAny<IEnumerable<DataRepository.DataEntities.Match>>())).ThrowsAsync(new DataException());

            var service = new MatchesService(_repository.Object, _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.BulkAddMatches(MockedMatches().ToList()));

            _repository.Verify(x => x.BatchUpsertAsync(It.IsAny<IEnumerable<DataRepository.DataEntities.Match>>()), Times.Once);
        }

        private IEnumerable<BusinessEntities.Match> MockedMatches()
        {
            return Enumerable.Range(1, 5).Select(id => new BusinessEntities.Match
            {
               Year = 2020, MatchId = (byte)id
            });
        }
    }
}
