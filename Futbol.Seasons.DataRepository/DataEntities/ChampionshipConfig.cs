using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace Futbol.Seasons.DataRepository.DataEntities
{
    [DynamoDBTable("Seasons")]
    public class ChampionshipConfig
    {
        [DynamoDBHashKey]
        public short Year { get; set; }

        [DynamoDBRangeKey]
        public string Sk { get; set; }

        [DynamoDBProperty]
        public string Name { get; set; }

        [DynamoDBProperty]
        public IEnumerable<byte> SeasonsIds{ get; set; }

        [DynamoDBIgnore]
        public IEnumerable<SeasonConfig> Seasons { get; set; }
    }
}