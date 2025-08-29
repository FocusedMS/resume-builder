using System.ComponentModel.DataAnnotations;

namespace ResumeApi.Dtos
{
    /// <summary>
    /// DTO for creating or updating a resume. Applies validation rules on
    /// content length and template selection.
    /// </summary>
    public class ResumeDto
    {
        [Required]
        [StringLength(160, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(8000)]
        public string? PersonalInfo { get; set; }
        = string.Empty;

        [MaxLength(16000)]
        public string? Education { get; set; }
        = string.Empty;

        [MaxLength(20000)]
        public string? Experience { get; set; }
        = string.Empty;

        [MaxLength(4000)]
        public string? Skills { get; set; }
        = string.Empty;

        [RegularExpression("^(classic|minimal|modern)$", ErrorMessage = "TemplateStyle must be one of: classic, minimal, modern.")]
        [StringLength(160)]
        public string TemplateStyle { get; set; } = "classic";
    }
}