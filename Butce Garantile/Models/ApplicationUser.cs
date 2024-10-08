using Microsoft.AspNetCore.Identity;

namespace Butce_Garantile.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
