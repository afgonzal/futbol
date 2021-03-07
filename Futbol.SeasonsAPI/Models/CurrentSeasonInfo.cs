
namespace Futbol.SeasonsAPI.Models
{
    public class CurrentSeasonInfo
    {
        public short Year { get; set; }
        public byte SeasonId { get; set; }

        public string Season { get; set; }
        public string Championship { get; set; }

        public byte? LastRound { get; set; }
        public byte? NextRound { get; set; }
    }
}
