using System.ComponentModel.DataAnnotations;

namespace MessageApp.Dtos
{
    public class UserLoginDto
    {
        [Required(ErrorMessage = "Username or Email is required.")]
        public string UsernameOrEmail { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&]).{8,}$", ErrorMessage = "Password must be at least 8 characters and contain at least one uppercase letter, one lowercase letter, one number, and one special character (!@#$%^&).")]
        public string Password { get; set; }
    }
}
