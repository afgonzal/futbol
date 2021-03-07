using Amazon.DynamoDBv2.DataModel;

namespace Futbol.Seasons.DataRepository.DataEntities
{
    [DynamoDBTable("Seasons")]
    public class CurrentSeasonConfig
    {
        [DynamoDBHashKey]
        public short Year { get; set; }

        [DynamoDBRangeKey] 
        public string Sk { get; set; }

        [DynamoDBProperty]
        public string CurrentSeason { get; set; }

        [DynamoDBIgnore]
        public short CurrentYear => CurrentSeason.ParseCompositeKey<short>(0, false).GetValueOrDefault();

        [DynamoDBIgnore] public byte Season => CurrentSeason.ParseCompositeKey<byte>(1, false).GetValueOrDefault();

        [DynamoDBProperty]
        public byte? LastRound { get; set; }

        [DynamoDBProperty]
        public byte NextRound { get; set; }

    }
}