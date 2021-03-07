using Amazon.DynamoDBv2.DataModel;

namespace Futbol.Seasons.DataRepository.DataEntities
{
    [DynamoDBTable("Teams")]
    public class TeamConferenceStats
    {
        private const string Prefix = "ConferenceStats";

        public TeamConferenceStats()
        {
            
        }

        public TeamConferenceStats(short year, byte conference,int teamId) : this()
        {
            Year = year;
            Sk = $"{Prefix}#{conference}#{teamId}";
        }
        [DynamoDBHashKey]
        public short Year { get; set; }

        [DynamoDBRangeKey]
        public string Sk { get; set; }

        [DynamoDBIgnore]
        public int TeamId => Sk.ParseCompositeKey<int>(2, false).GetValueOrDefault();

        [DynamoDBIgnore]
        public byte ConferenceId => Sk.ParseCompositeKey<byte>(1, false).GetValueOrDefault();

        [DynamoDBProperty]
        public string TeamName { get; set; }


        [DynamoDBProperty]
        public byte G { get; set; }

        [DynamoDBProperty]
        public byte W { get; set; }

        [DynamoDBProperty]
        public byte D { get; set; }

        [DynamoDBProperty]
        public byte L { get; set; }

        [DynamoDBProperty]
        public sbyte Sanctions { get; set; }

        [DynamoDBProperty]
        public byte GF { get; set; }

        [DynamoDBProperty]
        public byte GA { get; set; }

        [DynamoDBIgnore] 
        public sbyte GD => (sbyte)(GF - GA);

        [DynamoDBProperty] public byte Pts => (byte) (W * 3 + D);
    }
}