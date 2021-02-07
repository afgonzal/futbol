using System;
using System.Collections.Generic;
using System.Text;
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
    }
    public class SeasonConfigService : ISeasonConfigService
    {
        private readonly IChampionshipConfigRepository _championshipConfigRepository;
        private readonly ISeasonConfigRepository _configRepository;
        private readonly IMapper _mapper;

        public SeasonConfigService(IChampionshipConfigRepository championshipConfigRepository, ISeasonConfigRepository seasonConfigRepository, IMapper mapper)
        {
            _championshipConfigRepository = championshipConfigRepository;
            _configRepository = seasonConfigRepository;
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
    }
}
