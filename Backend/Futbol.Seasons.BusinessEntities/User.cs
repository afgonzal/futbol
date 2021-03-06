
namespace Futbol.Seasons.BusinessEntities
{
    public class User
    {
        public string Email { get; set; }

        public bool IsEnabled { get; set; }

        public UserRole Role { get; set; }
    }

    public enum UserRole
    {
        Admin, SetResults, ReadOnly
    }
}
