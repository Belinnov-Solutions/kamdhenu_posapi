using System;

using System.ComponentModel.DataAnnotations;
namespace BELEPOS.Entity.Auth
{

public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }

    }

}
