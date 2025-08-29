namespace ResumeApi.Models
{
    /// <summary>
    /// Represents a single AI suggestion returned to the user for a given section.
    /// </summary>
    public class Suggestion
    {
        public string Section { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ApplyTemplate { get; set; } = string.Empty;
    }
}