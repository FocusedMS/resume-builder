using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeApi.Models
{
    /// <summary>
    /// Entity representing a resume created by a user. Contains various sections
    /// of a resume plus metadata such as creation and update timestamps.
    /// </summary>
    public class Resume
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ResumeId { get; set; }

        /// <summary>
        /// Foreign key to the owning user. Required.
        /// </summary>
        [Required]
        public string UserId { get; set; } = default!;

        /// <summary>
        /// Title of the resume. Required, maximum length 160 characters.
        /// </summary>
        [Required]
        [MaxLength(160)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Personal information section (e.g. summary, contact). Up to 8k characters.
        /// </summary>
        [MaxLength(8000)]
        public string? PersonalInfo { get; set; }
        = string.Empty;

        /// <summary>
        /// Education section. Up to 16k characters.
        /// </summary>
        [MaxLength(16000)]
        public string? Education { get; set; }
        = string.Empty;

        /// <summary>
        /// Experience section. Up to 20k characters.
        /// </summary>
        [MaxLength(20000)]
        public string? Experience { get; set; }
        = string.Empty;

        /// <summary>
        /// Skills section. Up to 4k characters.
        /// </summary>
        [MaxLength(4000)]
        public string? Skills { get; set; }
        = string.Empty;

        /// <summary>
        /// Template style: classic, minimal or modern. Defaults to classic.
        /// </summary>
        [MaxLength(160)]
        public string TemplateStyle { get; set; } = "classic";

        /// <summary>
        /// JSON representation of AI suggestions returned to the user.
        /// </summary>
        public string? AiSuggestionsJson { get; set; }
        = null;

        /// <summary>
        /// UTC timestamp when the resume was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// UTC timestamp when the resume was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property to the owning user.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }
    }
}