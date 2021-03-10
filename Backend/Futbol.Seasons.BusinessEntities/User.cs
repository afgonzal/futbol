
using System.Collections.Generic;

namespace Futbol.Seasons.BusinessEntities
{
    public class User
    {
        public string Email { get; set; }

        public bool IsEnabled { get; set; }

        public IEnumerable<UserRole> Roles { get; set; }
    }

    public enum UserRole
    {
        Admin, Contributor, Editor, MatchEditor, Manager, Player, Social
    }
}
