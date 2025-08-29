using Microsoft.AspNetCore.Identity;

namespace ResumeApi.Models
{
    /// <summary>
    /// Application user extending the built‑in IdentityUser with a FullName field.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets the full name of the user. This value is optional.
        /// </summary>
        public string? FullName { get; set; }
    }
}