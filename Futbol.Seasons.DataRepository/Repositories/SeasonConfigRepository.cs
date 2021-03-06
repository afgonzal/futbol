﻿using Futbol.Seasons.DataRepository.DataEntities;
using Microsoft.Extensions.Configuration;

namespace Futbol.Seasons.DataRepository.Repositories
{
    public interface ISeasonConfigRepository : IRepository<SeasonConfig>
    {
    }

    public class SeasonConfigRepository : Repository<SeasonConfig>, ISeasonConfigRepository
    {
        private const string SeasonsTableName = "Seasons";
        public SeasonConfigRepository(IConfiguration config) : base(config, SeasonsTableName)
        {
        }
    }
}
