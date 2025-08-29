using System.ComponentModel.DataAnnotations;

namespace ResumeApi.Dtos
{
    /// <summary>
    /// DTO used when logging in an existing user.
    /// </summary>
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(128, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }
}