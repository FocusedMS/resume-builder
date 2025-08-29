using System.Text.RegularExpressions;
using ResumeApi.Models;

namespace ResumeApi.Services
{
    /// <summary>
    /// A simple heuristic implementation of the IAiSuggestionService. Generates
    /// structured suggestions based on the content length and presence of
    /// quantifiers in the resume sections. This service is offline and does not
    /// call any external AI providers.
    /// </summary>
    public class AiSuggestionService : IAiSuggestionService
    {
        public Task<IEnumerable<Suggestion>> GenerateSuggestionsAsync(Resume resume)
        {
            var suggestions = new List<Suggestion>();

            // Personal info too short
            var personal = resume.PersonalInfo ?? string.Empty;
            if (personal.Trim().Length < 160)
            {
                suggestions.Add(new Suggestion
                {
                    Section = "PersonalInfo",
                    Priority = "high",
                    Message = "Write a 2–3 sentence impact summary.",
                    ApplyTemplate = "Full‑stack developer with X years…"
                });
            }

            // Experience lacks numbers or percentages
            var experience = resume.Experience ?? string.Empty;
            if (!Regex.IsMatch(experience, "[0-9%]"))
            {
                suggestions.Add(new Suggestion
                {
                    Section = "Experience",
                    Priority = "med",
                    Message = "Quantify at least two bullets (%/ms/$/users).",
                    ApplyTemplate = "• Increased throughput by 25% by …"
                });
            }

            // Skills count heuristics
            var skills = (resume.Skills ?? string.Empty).Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                                          .Select(s => s.Trim())
                                                          .Where(s => !string.IsNullOrWhiteSpace(s))
                                                          .ToList();
            if (skills.Count < 8)
            {
                suggestions.Add(new Suggestion
                {
                    Section = "Skills",
                    Priority = "med",
                    Message = "Consider adding more relevant skills or grouping related skills together.",
                    ApplyTemplate = "Java, C#, SQL, Azure, React, Docker…"
                });
            }
            else if (skills.Count > 20)
            {
                suggestions.Add(new Suggestion
                {
                    Section = "Skills",
                    Priority = "low",
                    Message = "Trim or group skills to keep the section focused.",
                    ApplyTemplate = "Frontend: React, Angular; Backend: .NET, Node.js; DevOps: Docker, Kubernetes"
                });
            }

            // Missing education
            var education = resume.Education ?? string.Empty;
            if (string.IsNullOrWhiteSpace(education))
            {
                suggestions.Add(new Suggestion
                {
                    Section = "Education",
                    Priority = "med",
                    Message = "Add your degree, institution and graduation year.",
                    ApplyTemplate = "B.S. in Computer Science, XYZ University, 2023"
                });
            }

            return Task.FromResult((IEnumerable<Suggestion>)suggestions);
        }
    }
}