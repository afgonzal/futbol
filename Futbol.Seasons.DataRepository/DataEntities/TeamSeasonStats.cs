using Amazon.DynamoDBv2.DataModel;

namespace Futbol.Seasons.DataRepository.DataEntities
{
    [DynamoDBTable("Teams")]
    public class TeamSeasonStats
    {
        [DynamoDBHashKey]
        public short Year { get; set; }

        [DynamoDBRangeKey]
        internal string Sk { get; set; }

        [DynamoDBIgnore]
        public int TeamId
        {
            get => Sk.ParseCompositeKey<int>(1, false).GetValueOrDefault();
            set => Sk = $"SeasonStats#{value}";
        }

        [DynamoDBProperty]
        public string TeamName { get; set; }

        [DynamoDBProperty]
        public byte ConferenceId { get; set; }

        [DynamoDBProperty]
        public byte G { get; set; }

        [DynamoDBProperty]
        public byte W { get; set; }

        [DynamoDBProperty]
        public byte D { get; set; }

        [DynamoDBProperty]
        public byte L { get; set; }

        [DynamoDBProperty]
        public byte Sanctions { get; set; }

        [DynamoDBProperty]
        public byte GF { get; set; }

        [DynamoDBProperty]
        public byte GA { get; set; }

        [DynamoDBIgnore] 
        public byte GD => (byte)(GF - GA);

        [DynamoDBProperty] public byte Pts => (byte) (W * 3 + D);
    }
}