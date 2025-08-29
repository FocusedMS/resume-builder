using ResumeApi.Models;

namespace ResumeApi.Services
{
    /// <summary>
    /// Defines the interface for generating AI suggestions based on resume content.
    /// </summary>
    public interface IAiSuggestionService
    {
        Task<IEnumerable<Suggestion>> GenerateSuggestionsAsync(Resume resume);
    }
}