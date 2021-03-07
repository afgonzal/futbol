using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace Futbol.Seasons.DataRepository.DataEntities
{
    [DynamoDBTable("Seasons")]
    public class ConferenceConfig
    {
        [DynamoDBHashKey]
        public short Year { get; set; }

        [DynamoDBRangeKey]
        public string Sk { get; set; }

        [DynamoDBProperty]
        public string Name { get; set; }

        [DynamoDBProperty("Seasons")]
        public List<byte> SeasonsIds { get; set; }

        [DynamoDBProperty("ConferencesIds")]
        public List<byte> ConferencesIds { get; set; }


        [DynamoDBProperty("ConferencesNames")]
        public List<string> ConferencesNames{ get; set; }

        [DynamoDBIgnore]
        public IEnumerable<SeasonConfig> Seasons { get; set; }

        [DynamoDBIgnore]
        public IEnumerable<Conference> Conferences { get; set; }
    }

    public class Conference
    {
        public byte Id { get; set; }
        public string Name { get; set; }
    }
}