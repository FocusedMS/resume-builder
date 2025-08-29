using ResumeApi.Models;

namespace ResumeApi.Services
{
    /// <summary>
    /// Defines operations for generating PDF files from a resume.
    /// </summary>
    public interface IPdfService
    {
        /// <summary>
        /// Generate a PDF file (as a byte array) for the given resume using
        /// the specified template style.
        /// </summary>
        /// <param name="resume">Resume to render.</param>
        /// <param name="templateStyle">Template style: classic, minimal or modern.</param>
        /// <returns>Byte array representing the PDF document.</returns>
        Task<byte[]> GeneratePdfAsync(Resume resume, string templateStyle);
    }
}