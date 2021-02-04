using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace Futbol.Seasons.DataRepository.DataEntities
{
    [DynamoDBTable("Seasons")]
    public class YearConfig
    {
        [DynamoDBHashKey]
        public short Year { get; set; }

        [DynamoDBRangeKey]
        public string Sk { get; set; }


        [DynamoDBProperty]
        public IEnumerable<byte> Seasons{ get; set; }
    }
}