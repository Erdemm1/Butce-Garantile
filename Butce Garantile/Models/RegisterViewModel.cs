using System.ComponentModel.DataAnnotations;

namespace Butce_Garantile.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
    }


    public class LoginViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
