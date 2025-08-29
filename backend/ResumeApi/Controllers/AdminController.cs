using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeApi.Data;
using ResumeApi.Models;

namespace ResumeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _db;

        public AdminController(UserManager<ApplicationUser> userManager,
                               RoleManager<IdentityRole> roleManager,
                               AppDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        /// <summary>
        /// Get a list of all users along with their roles.
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var list = new List<object>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                list.Add(new { 
                    id = user.Id, 
                    email = user.Email, 
                    fullName = user.FullName, 
                    roles,
                    lockoutEnd = user.LockoutEnd,
                    isLocked = user.LockoutEnd > DateTimeOffset.UtcNow
                });
            }
            return Ok(list);
        }

        /// <summary>
        /// Get basic metrics about the system: total users, total resumes and
        /// resumes created in the last 24 hours.
        /// </summary>
        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalResumes = await _db.Resumes.CountAsync();
            var since = DateTime.UtcNow.AddHours(-24);
            var last24hResumes = await _db.Resumes.CountAsync(r => r.CreatedAt >= since);
            return Ok(new { totalUsers, totalResumes, last24hResumes });
        }

        /// <summary>
        /// Change the primary role of a user. Accepts a body containing the new role.
        /// </summary>
        [HttpPost("users/{id}/roles")]
        public async Task<IActionResult> ChangeRole(string id, [FromBody] dynamic body)
        {
            string role = body?.role;
            if (string.IsNullOrWhiteSpace(role))
            {
                return BadRequest(new { message = "Role is required." });
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var currentRoles = await _userManager.GetRolesAsync(user);
            // Remove existing roles
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
            await _userManager.AddToRoleAsync(user, role);
            return Ok(new { message = "Role updated" });
        }

        /// <summary>
        /// Add a role to a user without removing existing roles.
        /// </summary>
        [HttpPost("users/{id}/add-role")]
        public async Task<IActionResult> AddRole(string id, [FromBody] dynamic body)
        {
            string role = body?.role;
            if (string.IsNullOrWhiteSpace(role))
            {
                return BadRequest(new { message = "Role is required." });
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
            if (!await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }
            return Ok(new { message = "Role added" });
        }

        /// <summary>
        /// Remove a specific role from a user.
        /// </summary>
        [HttpDelete("users/{id}/roles/{role}")]
        public async Task<IActionResult> RemoveRole(string id, string role)
        {
            var me = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (me == id && role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return BadRequest("You cannot remove your own Admin role.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            if (await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.RemoveFromRoleAsync(user, role);
            }
            return Ok(new { message = "Role removed" });
        }

        /// <summary>
        /// Soft lock or unlock a user account. Accepts a body { lock: true/false }.
        /// </summary>
        [HttpPost("users/{id}/lock")]
        public async Task<IActionResult> LockUser(string id, [FromBody] dynamic body)
        {
            bool @lock = body?.locked ?? false;
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            if (@lock)
            {
                // Lock the user indefinitely
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            return Ok(new { message = @lock ? "User locked" : "User unlocked" });
        }

        /// <summary>
        /// Get all resumes in the system with comprehensive filtering, sorting, and pagination.
        /// Admin-only endpoint for global resume visibility.
        /// </summary>
        [HttpGet("resumes")]
        public async Task<IActionResult> GetAllResumes(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? q = null,
            [FromQuery] string? ownerEmail = null,
            [FromQuery] string? templateStyle = null,
            [FromQuery] string sortBy = "updatedAt",
            [FromQuery] string sortDir = "desc")
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            // Build query with joins to get owner information
            var query = from r in _db.Resumes
                       join u in _userManager.Users on r.UserId equals u.Id
                       select new
                       {
                           r.ResumeId,
                           r.Title,
                           r.TemplateStyle,
                           r.CreatedAt,
                           r.UpdatedAt,
                           r.PersonalInfo,
                           r.Education,
                           r.Experience,
                           r.Skills,
                           Owner = new
                           {
                               u.Id,
                               u.Email,
                               u.FullName
                           }
                       };

            // Apply filters
            if (!string.IsNullOrWhiteSpace(q))
            {
                            var searchTerm = q.ToLower();
            query = query.Where(x => 
                (x.Title != null && x.Title.ToLower().Contains(searchTerm)) ||
                (x.PersonalInfo != null && x.PersonalInfo.ToLower().Contains(searchTerm)) ||
                (x.Education != null && x.Education.ToLower().Contains(searchTerm)) ||
                (x.Experience != null && x.Experience.ToLower().Contains(searchTerm)) ||
                (x.Skills != null && x.Skills.ToLower().Contains(searchTerm)) ||
                (x.Owner.Email != null && x.Owner.Email.ToLower().Contains(searchTerm)) ||
                (x.Owner.FullName != null && x.Owner.FullName.ToLower().Contains(searchTerm))
            );
            }

            if (!string.IsNullOrWhiteSpace(ownerEmail))
            {
                query = query.Where(x => x.Owner.Email.ToLower().Contains(ownerEmail.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(templateStyle))
            {
                query = query.Where(x => x.TemplateStyle == templateStyle);
            }

            // Apply sorting
            query = (sortBy.ToLower(), sortDir.ToLower()) switch
            {
                ("createdat", "asc") => query.OrderBy(x => x.CreatedAt),
                ("createdat", _) => query.OrderByDescending(x => x.CreatedAt),
                ("updatedat", "asc") => query.OrderBy(x => x.UpdatedAt),
                ("updatedat", _) => query.OrderByDescending(x => x.UpdatedAt),
                ("title", "asc") => query.OrderBy(x => x.Title),
                ("title", _) => query.OrderByDescending(x => x.Title),
                ("owner", "asc") => query.OrderBy(x => x.Owner.Email),
                ("owner", _) => query.OrderByDescending(x => x.Owner.Email),
                _ => query.OrderByDescending(x => x.UpdatedAt)
            };

            // Get total count for pagination
            var total = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            // Calculate pagination metadata
            var totalPages = (int)Math.Ceiling((double)total / pageSize);
            var hasNextPage = page < totalPages;
            var hasPreviousPage = page > 1;

            return Ok(new
            {
                total,
                page,
                pageSize,
                totalPages,
                hasNextPage,
                hasPreviousPage,
                items
            });
        }

        /// <summary>
        /// Get a specific resume by ID with full details. Admin can view any resume.
        /// </summary>
        [HttpGet("resumes/{id}")]
        public async Task<IActionResult> GetResume(int id)
        {
            var resume = await _db.Resumes
                .Include(r => r.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ResumeId == id);

            if (resume == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                resume.ResumeId,
                resume.Title,
                resume.PersonalInfo,
                resume.Education,
                resume.Experience,
                resume.Skills,
                resume.TemplateStyle,
                resume.CreatedAt,
                resume.UpdatedAt,
                resume.AiSuggestionsJson,
                Owner = new
                {
                    resume.User.Id,
                    resume.User.Email,
                    resume.User.FullName
                }
            });
        }

        /// <summary>
        /// Get resume statistics and analytics for admin dashboard.
        /// </summary>
        [HttpGet("resumes/stats")]
        public async Task<IActionResult> GetResumeStats()
        {
            var now = DateTime.UtcNow;
            var last24h = now.AddHours(-24);
            var last7Days = now.AddDays(-7);
            var last30Days = now.AddDays(-30);

            var stats = await _db.Resumes
                .GroupBy(r => 1)
                .Select(g => new
                {
                    TotalResumes = g.Count(),
                    ResumesLast24h = g.Count(r => r.CreatedAt >= last24h),
                    ResumesLast7Days = g.Count(r => r.CreatedAt >= last7Days),
                    ResumesLast30Days = g.Count(r => r.CreatedAt >= last30Days),
                    AverageResumesPerDay = g.Count(r => r.CreatedAt >= last30Days) / 30.0,
                    MostActiveUsers = g.GroupBy(r => r.UserId)
                        .Select(ug => new
                        {
                            UserId = ug.Key,
                            ResumeCount = ug.Count()
                        })
                        .OrderByDescending(x => x.ResumeCount)
                        .Take(5)
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (stats != null)
            {
                return Ok(stats);
            }
            
            return Ok(new
            {
                TotalResumes = 0,
                ResumesLast24h = 0,
                ResumesLast7Days = 0,
                ResumesLast30Days = 0,
                AverageResumesPerDay = 0.0,
                MostActiveUsers = new List<object>()
            });
        }

        /// <summary>
        /// Get unique template styles used across all resumes.
        /// </summary>
        [HttpGet("resumes/templates")]
        public async Task<IActionResult> GetTemplateStyles()
        {
            var templates = await _db.Resumes
                .GroupBy(r => r.TemplateStyle)
                .Select(g => new
                {
                    TemplateStyle = g.Key,
                    Count = g.Count(),
                    LastUsed = g.Max(r => r.UpdatedAt)
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return Ok(templates);
        }

        /// <summary>
        /// Get user activity summary - how many resumes each user has created.
        /// </summary>
        [HttpGet("users/activity")]
        public async Task<IActionResult> GetUserActivity()
        {
            var userActivity = await _db.Resumes
                .GroupBy(r => r.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    ResumeCount = g.Count(),
                    LastActivity = g.Max(r => r.UpdatedAt),
                    FirstResume = g.Min(r => r.CreatedAt)
                })
                .OrderByDescending(x => x.ResumeCount)
                .ToListAsync();

            // Get user details for each entry
            var result = new List<object>();
            foreach (var activity in userActivity)
            {
                            var user = await _userManager.FindByIdAsync(activity.UserId);
            if (user != null)
            {
                result.Add(new
                {
                    activity.UserId,
                    activity.ResumeCount,
                    activity.LastActivity,
                    activity.FirstResume,
                    UserEmail = user.Email ?? "",
                    UserName = user.FullName ?? ""
                });
            }
            }

            return Ok(result);
        }
    }
}