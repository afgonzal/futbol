using Amazon.DynamoDBv2.DataModel;

namespace Futbol.Seasons.DataRepository.DataEntities
{
    [DynamoDBTable("Partners")]
    public class User
    {
        public const string UserSk = "Security";
        [DynamoDBHashKey] public string Email{ get; set; }

        [DynamoDBRangeKey] public string Sk => UserSk;

        [DynamoDBProperty]
        public bool IsEnabled { get; set; }

        [DynamoDBProperty]
        public string Role { get; set; }
    }
}