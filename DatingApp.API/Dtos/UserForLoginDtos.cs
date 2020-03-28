using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Dtos
{
    public class UserForLoginDtos
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(maximumLength: 8, MinimumLength = 4, ErrorMessage = @"Password should be in between 4 to 8 charcters.")]
        public string Password { get; set; }
    }
}
