using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ResumeApi.Models;

namespace ResumeApi.Data
{
    /// <summary>
    /// Entity Framework Core database context for the application. Inherits
    /// from IdentityDbContext to integrate ASP.NET Core Identity tables.
    /// </summary>
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Collection of resumes stored in the database.
        /// </summary>
        public DbSet<Resume> Resumes { get; set; } = default!;

        /// <summary>
        /// Configure entity model relationships and defaults.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Set default value for TemplateStyle if not supplied
            builder.Entity<Resume>()
                   .Property(r => r.TemplateStyle)
                   .HasDefaultValue("classic");
        }
    }
}