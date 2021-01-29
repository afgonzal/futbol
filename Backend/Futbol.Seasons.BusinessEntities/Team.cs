using System.Collections.Generic;

namespace Futbol.Seasons.BusinessEntities
{
    public class Team
    {
        public short Year { get; set; }
        public int? Id { get; set; }

        public string Name { get; set; }

        public byte ConferenceId { get; set; }

        public IEnumerable<short> Years { get; set; }

        public IEnumerable<string> Delegates { get; set; }

        public string Abbreviation { get; set; }
    }
}
