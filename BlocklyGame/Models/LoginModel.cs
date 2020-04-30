using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlocklyGame.Models
{
    public class LoginModel
    {
        [Required]
        [FromForm(Name = "login-username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [FromForm(Name = "login-password")]
        public string Password { get; set; }

        [FromForm(Name = "remember")]
        public bool RememberMe { get; set; }
    }
}
