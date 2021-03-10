using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace Futbol.Seasons.DataRepository.DataEntities
{
    [DynamoDBTable("Partners")]
    public class User
    {
        public const string UserSk = "Security";
        [DynamoDBHashKey] public string Email{ get; set; }

        [DynamoDBRangeKey] public string Sk
        {
            get
            {
                return UserSk;
            }
            set
            {

            }
        }

        [DynamoDBProperty]
        public bool IsEnabled { get; set; }

        [DynamoDBProperty]
        public List<string> Roles { get; set; }
    }
}