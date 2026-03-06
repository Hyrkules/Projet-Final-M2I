using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        public string Username;

        [Required]
        public string Password;
    }
}
