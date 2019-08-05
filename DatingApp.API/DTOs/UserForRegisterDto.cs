using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.DTOs
{
    public class UserForRegisterDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 7, ErrorMessage="Password must be between 7 and 20 characters.")]
        public string Password { get; set; }
    }
}