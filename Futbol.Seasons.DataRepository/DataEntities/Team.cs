using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace Futbol.Seasons.DataRepository.DataEntities
{
    [DynamoDBTable("Teams")]

    public class Team
    {
        [DynamoDBHashKey]
        public short Year { get; set; }

        [DynamoDBRangeKey]
        public int TeamId { get; set; }

        [DynamoDBProperty]
        public string TeamName { get; set; }

        [DynamoDBProperty]
        public byte ConferenceId { get; set; }

        [DynamoDBProperty]
        public List<short> Years { get; set; }

        [DynamoDBProperty]
        public List<string> Delegates { get; set; }

    }
}
