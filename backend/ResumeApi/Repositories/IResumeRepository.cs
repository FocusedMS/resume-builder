using ResumeApi.Models;

namespace ResumeApi.Repositories
{
    /// <summary>
    /// Defines the contract for data access operations on resumes. Implemented
    /// by EF Core repository and can be substituted for testing or alternative
    /// data stores.
    /// </summary>
    public interface IResumeRepository
    {
        Task<Resume?> GetByIdAsync(int id);
        Task<IEnumerable<Resume>> GetMineAsync(string userId);
        Task<IEnumerable<Resume>> GetAllAsync();
        Task<Resume> CreateAsync(Resume resume);
        Task UpdateAsync(Resume resume);
        Task DeleteAsync(Resume resume);
    }
}