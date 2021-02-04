using System;
using System.Collections.Generic;
using System.Text;
using Amazon.DynamoDBv2.DataModel;

namespace Futbol.Seasons.DataRepository.DataEntities
{
    [DynamoDBTable("Matches")]
    public class Match
    {
        [DynamoDBHashKey] public string YearSeasonRound { get; set; }

        [DynamoDBIgnore] public short Year => YearSeasonRound.ParseCompositeKey<short>(0, false).GetValueOrDefault();

        [DynamoDBIgnore] public byte Season => YearSeasonRound.ParseCompositeKey<byte>(1, false).GetValueOrDefault();

        [DynamoDBIgnore] public byte Round => YearSeasonRound.ParseCompositeKey<byte>(2, false).GetValueOrDefault();

        [DynamoDBRangeKey] public byte MatchId { get; set; }

        [DynamoDBProperty] public int HomeTeamId { get; set; }

        [DynamoDBProperty] public int AwayTeamId { get; set; }

        [DynamoDBProperty] public string HomeTeamName { get; set; }

        [DynamoDBProperty] public string AwayTeamName { get; set; }

        [DynamoDBProperty] public string ScheduledDate { get; set; }

        [DynamoDBProperty] public bool WasPlayed { get; set; }

        [DynamoDBProperty] public byte? HomeScore { get; set; }

        [DynamoDBProperty] public byte? AwayScore { get; set; }

        [DynamoDBProperty]
        public string HomeTeamIdYearSeason
        {
            get => $"{HomeTeamId}#{Year}#{Season}";
            set
            {
               
            }
        }

        [DynamoDBProperty]
        public string AwayTeamIdYearSeason
        {
            get => $"{AwayTeamId}#{Year}#{Season}";
            set {}
        }
    }

}
