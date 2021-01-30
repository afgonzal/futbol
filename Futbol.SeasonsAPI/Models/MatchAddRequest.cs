
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Futbol.SeasonsAPI.Models
{
    public class MatchAddRequest
    {
        public byte Round { get; set; }
        [JsonPropertyName("home")]
        public string HomeTeamAbbr { get; set; }
        [JsonPropertyName("away")]
        public string AwayTeamAbbr { get; set; }

        public int HomeTeamId { get; set; }

        public int AwayTeamId { get; set; }

    }
}
