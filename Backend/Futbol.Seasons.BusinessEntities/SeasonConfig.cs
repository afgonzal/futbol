using System.Collections.Generic;

namespace Futbol.Seasons.BusinessEntities
{
    public class SeasonConfig
    {
        public short Year { get; set; }


        public byte SeasonId { get; set; }

        public string Name { get; set; }

        public byte RoundsCount { get; set; }
    }

    public class ChampionshipConfig
    {
        public short Year { get; set; }

        public string Name { get; set; }

        public IEnumerable<SeasonConfig> Seasons { get; set; }
    }
}