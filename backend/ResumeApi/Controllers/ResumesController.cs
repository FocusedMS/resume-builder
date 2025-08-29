using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ResumeApi.Dtos;
using ResumeApi.Models;
using ResumeApi.Repositories;
using ResumeApi.Services;

namespace ResumeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ResumesController : ControllerBase
    {
        private readonly IResumeRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAiSuggestionService _aiService;
        private readonly IPdfService _pdfService;

        public ResumesController(
            IResumeRepository repository,
            UserManager<ApplicationUser> userManager,
            IAiSuggestionService aiService,
            IPdfService pdfService)
        {
            _repository = repository;
            _userManager = userManager;
            _aiService = aiService;
            _pdfService = pdfService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>
        /// List resumes belonging to the authenticated user. Admins can set
        /// ?all=1 to list all resumes in the system.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetResumes([FromQuery] bool? all)
        {
            var userId = GetUserId();
            var isAdmin = User.IsInRole("Admin");
            if (all == true && isAdmin)
            {
                var allResumes = await _repository.GetAllAsync();
                return Ok(allResumes);
            }
            var mine = await _repository.GetMineAsync(userId);
            return Ok(mine);
        }

        /// <summary>
        /// Get a single resume by id if the caller is the owner or an admin.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetResume(int id)
        {
            var resume = await _repository.GetByIdAsync(id);
            if (resume == null)
            {
                return NotFound();
            }
            var userId = GetUserId();
            var isAdmin = User.IsInRole("Admin");
            if (resume.UserId != userId && !isAdmin)
            {
                return Forbid();
            }
            return Ok(resume);
        }

        /// <summary>
        /// Create a new resume for the authenticated user.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ResumeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var sanitized = Sanitize(dto);
            if (sanitized.error != null)
            {
                return BadRequest(new { message = sanitized.error });
            }
            var resume = new Resume
            {
                UserId = GetUserId(),
                Title = sanitized.value.Title!,
                PersonalInfo = sanitized.value.PersonalInfo,
                Education = sanitized.value.Education,
                Experience = sanitized.value.Experience,
                Skills = sanitized.value.Skills,
                TemplateStyle = sanitized.value.TemplateStyle,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _repository.CreateAsync(resume);
            return Ok(resume);
        }

        /// <summary>
        /// Update an existing resume. Only owners or admins may update.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ResumeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var resume = await _repository.GetByIdAsync(id);
            if (resume == null)
            {
                return NotFound();
            }
            var userId = GetUserId();
            var isAdmin = User.IsInRole("Admin");
            if (resume.UserId != userId && !isAdmin)
            {
                return Forbid();
            }
            var sanitized = Sanitize(dto);
            if (sanitized.error != null)
            {
                return BadRequest(new { message = sanitized.error });
            }
            resume.Title = sanitized.value.Title!;
            resume.PersonalInfo = sanitized.value.PersonalInfo;
            resume.Education = sanitized.value.Education;
            resume.Experience = sanitized.value.Experience;
            resume.Skills = sanitized.value.Skills;
            resume.TemplateStyle = sanitized.value.TemplateStyle;
            resume.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(resume);
            return Ok(resume);
        }

        /// <summary>
        /// Delete a resume. Only owners or admins may delete.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var resume = await _repository.GetByIdAsync(id);
            if (resume == null)
            {
                return NotFound();
            }
            var userId = GetUserId();
            var isAdmin = User.IsInRole("Admin");
            if (resume.UserId != userId && !isAdmin)
            {
                return Forbid();
            }
            await _repository.DeleteAsync(resume);
            return NoContent();
        }

        /// <summary>
        /// Generate and download a PDF for the specified resume.
        /// </summary>
        [HttpGet("download/{id:int}")]
        public async Task<IActionResult> Download(int id)
        {
            var resume = await _repository.GetByIdAsync(id);
            if (resume == null)
            {
                return NotFound();
            }
            var userId = GetUserId();
            var isAdmin = User.IsInRole("Admin");
            if (resume.UserId != userId && !isAdmin)
            {
                return Forbid();
            }
            var pdfBytes = await _pdfService.GeneratePdfAsync(resume, resume.TemplateStyle);
            var fileName = $"resume-{resume.ResumeId}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        /// <summary>
        /// Generate AI suggestions for the specified resume. Persists the
        /// suggestions JSON into the resume record.
        /// </summary>
        [HttpPost("{id:int}/ai-suggestions")]
        public async Task<IActionResult> GenerateSuggestions(int id)
        {
            var resume = await _repository.GetByIdAsync(id);
            if (resume == null)
            {
                return NotFound();
            }
            var userId = GetUserId();
            var isAdmin = User.IsInRole("Admin");
            if (resume.UserId != userId && !isAdmin)
            {
                return Forbid();
            }
            var suggestions = await _aiService.GenerateSuggestionsAsync(resume);
            var json = JsonConvert.SerializeObject(suggestions, Formatting.Indented);
            resume.AiSuggestionsJson = json;
            resume.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(resume);
            return Ok(new { suggestions });
        }

        /// <summary>
        /// Sanitize and validate the aggregate length of text fields. Rejects
        /// malicious content and large payloads. Returns an error message if
        /// invalid otherwise the sanitized dto.
        /// </summary>
        private (ResumeDto value, string? error) Sanitize(ResumeDto dto)
        {
            // Helper to trim, normalise newlines, strip control characters
            string Clean(string? input)
            {
                if (string.IsNullOrWhiteSpace(input)) return string.Empty;
                
                var text = input.Trim();
                // Normalize line endings
                text = text.Replace("\r\n", "\n").Replace("\r", "\n");
                
                var filtered = new System.Text.StringBuilder();
                foreach (var ch in text)
                {
                    // Allow only safe control characters
                    if (char.IsControl(ch) && ch != '\n' && ch != '\t')
                        continue;
                    filtered.Append(ch);
                }
                return filtered.ToString();
            }
            
            // Reject script, iframe, and other dangerous tags
            bool ContainsDangerous(string text)
            {
                if (string.IsNullOrWhiteSpace(text)) return false;
                
                var lower = text.ToLowerInvariant();
                var dangerousPatterns = new[]
                {
                    "<script", "</script>", "<iframe", "</iframe>", 
                    "javascript:", "vbscript:", "onload=", "onerror=",
                    "onclick=", "onmouseover=", "eval(", "document.cookie"
                };
                
                return dangerousPatterns.Any(pattern => lower.Contains(pattern));
            }
            
            // Clean all fields
            var p = Clean(dto.PersonalInfo);
            var e = Clean(dto.Education);
            var ex = Clean(dto.Experience);
            var s = Clean(dto.Skills);
            var t = Clean(dto.Title);
            
            // Check for dangerous content
            if (ContainsDangerous(p) || ContainsDangerous(e) || ContainsDangerous(ex) || ContainsDangerous(s) || ContainsDangerous(t))
            {
                return (dto, "Input contains prohibited content or tags.");
            }
            
            // Enforce payload size limit (40,000 chars across all text fields)
            var totalLen = p.Length + e.Length + ex.Length + s.Length + t.Length;
            if (totalLen > 40000)
            {
                return (dto, $"The combined length of text fields ({totalLen:N0}) exceeds the maximum limit of 40,000 characters.");
            }
            
            // Validate template style
            var validTemplates = new[] { "classic", "minimal", "modern" };
            var templateStyle = validTemplates.Contains(dto.TemplateStyle) ? dto.TemplateStyle : "classic";
            
            var sanitized = new ResumeDto
            {
                Title = t,
                PersonalInfo = p,
                Education = e,
                Experience = ex,
                Skills = s,
                TemplateStyle = templateStyle
            };
            
            return (sanitized, null);
        }
    }
}