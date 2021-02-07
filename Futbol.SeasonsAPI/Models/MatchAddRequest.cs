
using System;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Futbol.SeasonsAPI.Models
{
    public class MatchModel
    {
        public short Year { get; set; }
        public byte Season { get; set; }
        public byte Round { get; set; }
        public byte MatchId { get; set; }
        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }
        public string HomeTeamName { get; set; }
        public string AwayTeamName { get; set; }

        public bool WasPlayed { get; set; }
        public DateTimeOffset ScheduledDate { get; set; }
        public byte? HomeScore { get; set; }
        public byte? AwayScore { get; set; }

    }
    public class MatchAddRequest
    {
        public byte Round { get; set; }
        [JsonPropertyName("home")]
        public string HomeTeamAbbr { get; set; }
        [JsonPropertyName("away")]
        public string AwayTeamAbbr { get; set; }

        public int HomeTeamId { get; set; }

        public int AwayTeamId { get; set; }
        public string HomeTeamName { get; set; }
        public string AwayTeamName { get; set; }
        [JsonPropertyName("sd")]
        public string ScheduledDate { get; set; }
    }

    public class MatchResultRequest : MatchAddRequest
    {
        public byte? HomeScore { get; set; }
        public byte? AwayScore { get; set; }
      
      
    }
}
