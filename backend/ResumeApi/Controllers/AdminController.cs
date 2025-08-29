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
                list.Add(new { id = user.Id, email = user.Email, fullName = user.FullName, roles });
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
    }
}