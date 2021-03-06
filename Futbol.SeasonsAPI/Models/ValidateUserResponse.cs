using Futbol.Seasons.BusinessEntities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Futbol.SeasonsAPI.Models
{
    public class ValidateUserResponse
    {
        public bool IsAuthorized { get; set; }
        public string Token { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public UserRole Role { get; set; }
    }
}
