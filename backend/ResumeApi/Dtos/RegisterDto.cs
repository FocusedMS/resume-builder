using System.ComponentModel.DataAnnotations;

namespace ResumeApi.Dtos
{
    /// <summary>
    /// DTO used when registering a new user. Performs basic validation on
    /// submitted fields.
    /// </summary>
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(128, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [RegularExpression("^[a-zA-Z0-9 .'\\-]{2,100}$", ErrorMessage = "Full name contains invalid characters.")]
        [StringLength(100, MinimumLength = 2)]
        public string? FullName { get; set; }
        = null;
    }
}