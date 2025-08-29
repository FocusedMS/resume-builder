using System.Text.RegularExpressions;
using ResumeApi.Models;

namespace ResumeApi.Services
{
    /// <summary>
    /// A comprehensive AI suggestion service that analyzes resume content and provides
    /// structured, actionable suggestions for improvement. This service uses heuristics
    /// to identify areas for enhancement without requiring external AI services.
    /// </summary>
    public class AiSuggestionService : IAiSuggestionService
    {
        public Task<IEnumerable<Suggestion>> GenerateSuggestionsAsync(Resume resume)
        {
            var suggestions = new List<Suggestion>();

            // Personal Info Analysis
            AnalyzePersonalInfo(resume, suggestions);
            
            // Experience Analysis
            AnalyzeExperience(resume, suggestions);
            
            // Skills Analysis
            AnalyzeSkills(resume, suggestions);
            
            // Education Analysis
            AnalyzeEducation(resume, suggestions);
            
            // Content Quality Analysis
            AnalyzeContentQuality(resume, suggestions);

            return Task.FromResult((IEnumerable<Suggestion>)suggestions);
        }

        private void AnalyzePersonalInfo(Resume resume, List<Suggestion> suggestions)
        {
            var personal = resume.PersonalInfo ?? string.Empty;
            var trimmed = personal.Trim();
            
            if (trimmed.Length < 100)
            {
                suggestions.Add(new Suggestion
                {
                    Section = "PersonalInfo",
                    Priority = "high",
                    Message = "Your personal summary is too brief. Write a compelling 2-3 sentence professional summary that highlights your key strengths and career objectives.",
                    ApplyTemplate = "Experienced full-stack developer with 5+ years building scalable web applications using modern technologies. Passionate about clean code, user experience, and continuous learning. Seeking opportunities to lead development teams and architect innovative solutions."
                });
            }
            else if (trimmed.Length < 200)
            {
                suggestions.Add(new Suggestion
                {
                    Section = "PersonalInfo",
                    Priority = "medium",
                    Message = "Consider expanding your personal summary to better showcase your unique value proposition.",
                    ApplyTemplate = "Add specific achievements, certifications, or career goals to make your summary more compelling."
                });
            }
            
            // Check for impact words
            var impactWords = new[] { "achieved", "improved", "increased", "developed", "led", "managed", "delivered" };
            if (!impactWords.Any(word => trimmed.ToLower().Contains(word)))
            {
                suggestions.Add(new Suggestion
                {
                    Section = "PersonalInfo",
                    Priority = "medium",
                    Message = "Use action-oriented words to make your summary more impactful.",
                    ApplyTemplate = "Replace passive language with strong action verbs like 'achieved', 'improved', 'developed', or 'led'."
                });
            }
        }

        private void AnalyzeExperience(Resume resume, List<Suggestion> suggestions)
        {
            var experience = resume.Experience ?? string.Empty;
            var trimmed = experience.Trim();
            
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                suggestions.Add(new Suggestion
                {
                    Section = "Experience",
                    Priority = "high",
                    Message = "Experience section is empty. Add your work history with specific achievements and responsibilities.",
                    ApplyTemplate = "Software Developer | Company Name | 2020-2023\n• Developed and maintained web applications using React and .NET\n• Collaborated with cross-functional teams to deliver features\n• Improved application performance by 30% through optimization"
                });
                return;
            }

            // Check for quantifiable achievements
            var hasNumbers = Regex.IsMatch(trimmed, @"\d+");
            var hasPercentages = trimmed.Contains("%");
            var hasDollarAmounts = Regex.IsMatch(trimmed, @"\$[\d,]+");
            
            if (!hasNumbers && !hasPercentages && !hasDollarAmounts)
            {
                suggestions.Add(new Suggestion
                {
                    Section = "Experience",
                    Priority = "high",
                    Message = "Add specific numbers and metrics to quantify your achievements. This makes your experience more compelling.",
                    ApplyTemplate = "• Increased user engagement by 25% through UI/UX improvements\n• Reduced application load time by 40% (from 3s to 1.8s)\n• Managed team of 5 developers and delivered 12 features on schedule"
                });
            }
            
            // Check for action verbs
            var actionVerbs = new[] { "developed", "implemented", "designed", "managed", "led", "created", "optimized", "deployed" };
            var hasActionVerbs = actionVerbs.Any(verb => trimmed.ToLower().Contains(verb));
            
            if (!hasActionVerbs)
            {
                suggestions.Add(new Suggestion
                {
                    Section = "Experience",
                    Priority = "medium",
                    Message = "Start each bullet point with strong action verbs to demonstrate your proactive approach.",
                    ApplyTemplate = "• Developed new features using React and TypeScript\n• Implemented CI/CD pipeline reducing deployment time by 60%\n• Led code reviews and mentored junior developers"
                });
            }
        }

        private void AnalyzeSkills(Resume resume, List<Suggestion> suggestions)
        {
            var skills = resume.Skills ?? string.Empty;
            if (string.IsNullOrWhiteSpace(skills))
            {
                suggestions.Add(new Suggestion
                {
                    Section = "Skills",
                    Priority = "high",
                    Message = "Skills section is empty. Add relevant technical and soft skills.",
                    ApplyTemplate = "Technical: JavaScript, React, .NET Core, SQL, Git\nSoft Skills: Leadership, Communication, Problem Solving, Team Collaboration"
                });
                return;
            }

            var skillList = skills.Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim())
                                .Where(s => !string.IsNullOrWhiteSpace(s))
                                .ToList();

            if (skillList.Count < 6)
            {
                suggestions.Add(new Suggestion
                {
                    Section = "Skills",
                    Priority = "medium",
                    Message = "Consider adding more relevant skills to demonstrate your breadth of knowledge.",
                    ApplyTemplate = "Add skills like: Docker, AWS, REST APIs, Unit Testing, Agile/Scrum, Database Design"
                });
            }
            else if (skillList.Count > 25)
            {
                suggestions.Add(new Suggestion
                {
                    Section = "Skills",
                    Priority = "low",
                    Message = "Too many skills can dilute your expertise. Focus on your core competencies and most relevant skills.",
                    ApplyTemplate = "Group related skills: 'Frontend: React, Angular, Vue | Backend: .NET, Node.js | DevOps: Docker, AWS, Azure'"
                });
            }

            // Check for skill categorization
            if (!skills.Contains(":") && skillList.Count > 8)
            {
                suggestions.Add(new Suggestion
                {
                    Section = "Skills",
                    Priority = "low",
                    Message = "Consider grouping your skills by category for better organization.",
                    ApplyTemplate = "Programming Languages: C#, JavaScript, Python\nFrameworks: .NET Core, React, Angular\nTools: Git, Docker, Azure DevOps"
                });
            }
        }

        private void AnalyzeEducation(Resume resume, List<Suggestion> suggestions)
        {
            var education = resume.Education ?? string.Empty;
            var trimmed = education.Trim();
            
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                suggestions.Add(new Suggestion
                {
                    Section = "Education",
                    Priority = "medium",
                    Message = "Add your educational background including degree, institution, and graduation year.",
                    ApplyTemplate = "Bachelor of Science in Computer Science\nUniversity Name | Graduated: 2023\nRelevant Coursework: Data Structures, Algorithms, Database Systems"
                });
            }
            else if (trimmed.Length < 50)
            {
                suggestions.Add(new Suggestion
                {
                    Section = "Education",
                    Priority = "low",
                    Message = "Consider adding more details about your education, including relevant coursework or achievements.",
                    ApplyTemplate = "Add: GPA (if 3.5+), relevant coursework, honors, certifications, or academic projects."
                });
            }
        }

        private void AnalyzeContentQuality(Resume resume, List<Suggestion> suggestions)
        {
            var allContent = $"{resume.PersonalInfo} {resume.Education} {resume.Experience} {resume.Skills}".ToLower();
            
            // Check for buzzwords
            var buzzwords = new[] { "synergy", "leverage", "paradigm", "streamline", "optimize", "facilitate" };
            var buzzwordCount = buzzwords.Count(word => allContent.Contains(word));
            
            if (buzzwordCount > 2)
            {
                suggestions.Add(new Suggestion
                {
                    Section = "Content",
                    Priority = "low",
                    Message = "Reduce corporate buzzwords. Use clear, specific language that directly describes your achievements.",
                    ApplyTemplate = "Replace buzzwords with specific actions: 'led a team' instead of 'facilitated team synergy'"
                });
            }
            
            // Check for consistent formatting
            var hasBulletPoints = allContent.Contains("•") || allContent.Contains("-");
            if (!hasBulletPoints && resume.Experience?.Length > 100)
            {
                suggestions.Add(new Suggestion
                {
                    Section = "Formatting",
                    Priority = "medium",
                    Message = "Use bullet points to make your experience section more scannable and professional.",
                    ApplyTemplate = "Convert paragraphs to bullet points:\n• [Specific achievement or responsibility]\n• [Another achievement or responsibility]"
                });
            }
        }
    }
}