using System;
using System.Collections.Generic;
using System.Text;
using Amazon.DynamoDBv2.DataModel;

namespace Futbol.Seasons.DataRepository.DataEntities
{
    [DynamoDBTable("Teams")]

    public class Team
    {
        [DynamoDBHashKey]
        public string YearTeamId { get; set; }

        [DynamoDBIgnore]
        public short Year => YearTeamId.ParseCompositeKey<short>(0, false).GetValueOrDefault();

        [DynamoDBIgnore] public int TeamId => YearTeamId.ParseCompositeKey<int>(1, false).GetValueOrDefault();

        [DynamoDBProperty]
        public string TeamName { get; set; }

        [DynamoDBProperty]
        public byte ConferenceId { get; set; }

        [DynamoDBProperty]
        public List<int> Years { get; set; }

        [DynamoDBProperty]
        public List<string> Delegates { get; set; }

    }
}
