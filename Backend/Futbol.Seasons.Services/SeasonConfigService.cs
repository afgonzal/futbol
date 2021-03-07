using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Futbol.Seasons.BusinessEntities;
using Futbol.Seasons.DataRepository.Repositories;

namespace Futbol.Seasons.Services
{
    public interface ISeasonConfigService
    {
        Task<SeasonConfig> GetConfig(short year, byte season);
        Task<ChampionshipConfig> GetConfig(short year);
        Task<ConferenceConfig> GetConferenceConfig(short year, byte conferenceId);
        Task<IEnumerable<ConferenceConfig>> GetConferencesConfig(short year);
    }
    public class SeasonConfigService : ISeasonConfigService
    {
        private readonly IChampionshipConfigRepository _championshipConfigRepository;
        private readonly ISeasonConfigRepository _configRepository;
        private readonly IConferenceConfigRepository _conferenceConfigRepository;
        private readonly IMapper _mapper;

        public SeasonConfigService(IChampionshipConfigRepository championshipConfigRepository, ISeasonConfigRepository seasonConfigRepository, IConferenceConfigRepository conferenceConfigRepository, IMapper mapper)
        {
            _championshipConfigRepository = championshipConfigRepository;
            _configRepository = seasonConfigRepository;
            _conferenceConfigRepository = conferenceConfigRepository;
            _mapper = mapper;
        }

        public async Task<SeasonConfig> GetConfig(short year, byte season)
        {
            var config = await _configRepository.GetByKeyAsync(year, $"Season#{season}");
            return _mapper.Map<SeasonConfig>(config);
        }

        public async Task<ChampionshipConfig> GetConfig(short year)
        {
            var config = await _championshipConfigRepository.GetYearConfig(year);
            return _mapper.Map<ChampionshipConfig>(config);
        }

        public async Task<IEnumerable<ConferenceConfig>> GetConferencesConfig(short year)
        {
            var conferences = await _conferenceConfigRepository.GetConferencesConfig(year);

            return conferences.Conferences.Select(conf =>  new ConferenceConfig
            {
                Id = conf.Id,
                Name = conf.Name,
                Year = year,
                Seasons = _mapper.Map<IEnumerable<SeasonConfig>>(conferences.Seasons)
            });
        }

        public async Task<ConferenceConfig> GetConferenceConfig(short year, byte conferenceId)
        {
            var conferences = await _conferenceConfigRepository.GetConferencesConfig(year);

            var conference = conferences.Conferences.Single(c => c.Id == conferenceId);
            return new ConferenceConfig
            {
                Id = conferenceId, Name = conference.Name, Year = year,
                Seasons = _mapper.Map<IEnumerable<SeasonConfig>>(conferences.Seasons)
            };
        }
    }
}
