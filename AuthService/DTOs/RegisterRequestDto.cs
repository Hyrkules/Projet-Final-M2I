using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class RegisterRequestDto
    {
        [Required]
        [MinLength(3)]
        public string Username;
        [Required][MinLength(6)]
        string Password;
        [Required]
        [EmailAddress]
        public string Email;
    }
}
