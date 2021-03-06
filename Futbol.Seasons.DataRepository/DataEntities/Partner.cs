using System;
using Amazon.DynamoDBv2.DataModel;

namespace Futbol.Seasons.DataRepository.DataEntities
{
    [DynamoDBTable("Partners")]
    public class Partner
    {
        public const string PartnerSk = "Profile";
        [DynamoDBHashKey]
        public string Email { get; set; }

        [DynamoDBRangeKey] public string Sk => PartnerSk;

        [DynamoDBProperty]
        public string FirstName { get; set; }

        [DynamoDBProperty]
        public string LastName { get; set; }

        [DynamoDBProperty]

        public long OldId{ get; set; }

        [DynamoDBProperty]
        public string Phone { get; set; }

        [DynamoDBProperty]
        public short YearJoined { get; set; }

        [DynamoDBProperty]

        public DateTime DoB { get; set; }

        [DynamoDBProperty]
        public long CI { get; set; }

        [DynamoDBProperty]
        public string Twitter { get; set; }

        [DynamoDBProperty]
        public string Instagram { get; set; }
    }
}
