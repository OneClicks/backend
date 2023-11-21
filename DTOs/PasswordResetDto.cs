using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class PasswordResetDto
    {
        [Required]
        public string Token { get; set; }

        [Required, MinLength(6, ErrorMessage = "PLease enter at least 6 characters")]
        public string Password { get; set; }

        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
