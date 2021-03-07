using Amazon.DynamoDBv2.DataModel;

namespace Futbol.Seasons.DataRepository.DataEntities
{
    [DynamoDBTable("Seasons")]
    public class SeasonConfig
    {
        [DynamoDBHashKey]
        public short Year { get; set; }

        [DynamoDBRangeKey]
        public string Sk { get; set; }

        [DynamoDBIgnore]
        public byte SeasonId
        {
            get => Sk.ParseCompositeKey<byte>(1, false).GetValueOrDefault();
            set => Sk = $"Season#{value}";
        }

        [DynamoDBProperty]
        public string Name { get; set; }

        [DynamoDBProperty] 
        public byte RoundsCount { get; set; }
    }
}