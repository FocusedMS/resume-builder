using Microsoft.EntityFrameworkCore;
using ResumeApi.Data;
using ResumeApi.Models;

namespace ResumeApi.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the IResumeRepository interface. This
    /// class encapsulates CRUD operations for resumes and hides underlying
    /// DbContext details from consumers.
    /// </summary>
    public class ResumeRepository : IResumeRepository
    {
        private readonly AppDbContext _context;
        public ResumeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Resume?> GetByIdAsync(int id)
        {
            return await _context.Resumes.FirstOrDefaultAsync(r => r.ResumeId == id);
        }

        public async Task<IEnumerable<Resume>> GetMineAsync(string userId)
        {
            return await _context.Resumes
                                 .Where(r => r.UserId == userId)
                                 .OrderByDescending(r => r.UpdatedAt)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Resume>> GetAllAsync()
        {
            return await _context.Resumes
                                 .OrderByDescending(r => r.UpdatedAt)
                                 .ToListAsync();
        }

        public async Task<Resume> CreateAsync(Resume resume)
        {
            _context.Resumes.Add(resume);
            await _context.SaveChangesAsync();
            return resume;
        }

        public async Task UpdateAsync(Resume resume)
        {
            _context.Resumes.Update(resume);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Resume resume)
        {
            _context.Resumes.Remove(resume);
            await _context.SaveChangesAsync();
        }
    }
}