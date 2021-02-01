using System;
using System.Collections.Generic;
using System.Text;

namespace Futbol.Seasons.BusinessEntities
{
    public class Match
    {
        public short Year { get; set; }
        public byte Season { get; set; }

        public byte Round { get; set; }

        public byte MatchId { get; set; }

        public int HomeTeamId { get; set; }

        public int AwayTeamId { get; set; }

        public string HomeTeamName { get; set; }

        public string AwayTeamName { get; set; }

        public DateTimeOffset ScheduledDate { get; set; }

        public bool WasPlayed { get; set; }

        public byte? HomeScore { get; set; }

        public byte? AwayScore { get; set; }

    }

}
