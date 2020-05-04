using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace BlocklyGame.Models
{
    public class RegisterModel
    {
        [Required]
        [FromForm(Name = "register-username")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [FromForm(Name = "register-password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [FromForm(Name = "register-password_confirmation")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
