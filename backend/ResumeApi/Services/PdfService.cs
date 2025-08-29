using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using ResumeApi.Models;

namespace ResumeApi.Services
{
    /// <summary>
    /// Generates simple PDF documents for resumes using iText7. Provides three
    /// basic styles (classic, minimal, modern) with different colour palettes
    /// and font treatments.
    /// </summary>
    public class PdfService : IPdfService
    {
        public Task<byte[]> GeneratePdfAsync(Resume resume, string templateStyle)
        {
            using var ms = new MemoryStream();
            using var writer = new PdfWriter(ms);
            using var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Choose a primary colour based on template style
            Color primaryColor = templateStyle switch
            {
                "modern" => WebColors.GetRGBColor("#0D47A1"),
                "minimal" => WebColors.GetRGBColor("#1E88E5"),
                _ => WebColors.GetRGBColor("#0F172A")
            };

            // Title
            var title = new Paragraph(resume.Title)
                .SetFontSize(18)
                .SetBold()
                .SetFontColor(primaryColor)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(title);
            document.Add(new Paragraph("\n"));

            // Helper to append a section header and body
            void AddSection(string header, string? content)
            {
                if (string.IsNullOrWhiteSpace(content)) return;
                var h = new Paragraph(header)
                    .SetBold()
                    .SetFontSize(14)
                    .SetFontColor(primaryColor);
                document.Add(h);
                var p = new Paragraph(content!.Trim())
                    .SetFontSize(11)
                    .SetMarginBottom(8);
                document.Add(p);
            }

            AddSection("Personal Information", resume.PersonalInfo);
            AddSection("Education", resume.Education);
            AddSection("Experience", resume.Experience);
            AddSection("Skills", resume.Skills);

            document.Close();
            return Task.FromResult(ms.ToArray());
        }
    }
}