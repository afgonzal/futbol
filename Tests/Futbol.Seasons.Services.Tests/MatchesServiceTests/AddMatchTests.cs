using System.Data;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.DataRepository.Repositories;
using Moq;
using NUnit.Framework;

namespace Futbol.Seasons.Services.Tests.MatchesServiceTests
{
    [TestFixture]
    public class AddMatchTests
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
            var service = new MatchesService(_repository.Object, null, _mapper);
            await service.AddMatch(MockedMatch());
            _repository.Verify(x => x.AddAsync(It.IsAny<DataRepository.DataEntities.Match>()),Times.Once);
        }

        [Test]
        public void RepositoryError_ThrowException()
        {
            _repository.Setup(x => x.AddAsync(It.IsAny<DataRepository.DataEntities.Match>())).ThrowsAsync(new DataException());

            var service = new MatchesService(_repository.Object, null, _mapper);
            Assert.ThrowsAsync<DataException>(async () => await service.AddMatch(MockedMatch()));

            _repository.Verify(x => x.AddAsync(It.IsAny<DataRepository.DataEntities.Match>()), Times.Once);
        }

        private BusinessEntities.Match MockedMatch()
        {
            return new BusinessEntities.Match
            {
               Year =  2020, MatchId = 3
            };
        }
    }
}
