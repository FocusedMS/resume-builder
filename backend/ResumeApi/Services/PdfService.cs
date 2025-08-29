using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using ResumeApi.Models;

namespace ResumeApi.Services
{
    /// <summary>
    /// Generates professional PDF documents for resumes using iText7. Provides three
    /// distinct styles (classic, minimal, modern) with different colour palettes,
    /// typography, and layout approaches.
    /// </summary>
    public class PdfService : IPdfService
    {
        public Task<byte[]> GeneratePdfAsync(Resume resume, string templateStyle)
        {
            using var ms = new MemoryStream();
            using var writer = new PdfWriter(ms);
            using var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Set margins based on template style
            var margins = templateStyle switch
            {
                "minimal" => new float[] { 50, 50, 50, 50 }, // Equal margins
                "modern" => new float[] { 40, 60, 40, 60 },   // Wider side margins
                _ => new float[] { 50, 50, 50, 50 }           // Classic: standard margins
            };
            document.SetMargins(margins[0], margins[1], margins[2], margins[3]);

            // Choose colors and fonts based on template style
            var (primaryColor, secondaryColor, backgroundColor) = GetTemplateColors(templateStyle);
            var (titleFont, headerFont, bodyFont) = GetTemplateFonts(templateStyle);

            // Add header based on template style
            AddHeader(document, resume, templateStyle, primaryColor, titleFont);

            // Add content sections based on template style
            switch (templateStyle)
            {
                case "minimal":
                    AddMinimalLayout(document, resume, primaryColor, headerFont, bodyFont);
                    break;
                case "modern":
                    AddModernLayout(document, resume, primaryColor, secondaryColor, headerFont, bodyFont);
                    break;
                default:
                    AddClassicLayout(document, resume, primaryColor, headerFont, bodyFont);
                    break;
            }

            document.Close();
            return Task.FromResult(ms.ToArray());
        }

        private (Color primary, Color secondary, Color background) GetTemplateColors(string templateStyle)
        {
            return templateStyle switch
            {
                "modern" => (
                    WebColors.GetRGBColor("#0D47A1"),    // Deep Blue
                    WebColors.GetRGBColor("#1976D2"),    // Medium Blue
                    WebColors.GetRGBColor("#F5F5F5")     // Light Gray
                ),
                "minimal" => (
                    WebColors.GetRGBColor("#1E88E5"),    // Blue
                    WebColors.GetRGBColor("#42A5F5"),    // Light Blue
                    WebColors.GetRGBColor("#FFFFFF")     // White
                ),
                _ => (
                    WebColors.GetRGBColor("#0F172A"),    // Dark Blue
                    WebColors.GetRGBColor("#334155"),    // Medium Gray
                    WebColors.GetRGBColor("#FFFFFF")     // White
                )
            };
        }

        private (PdfFont title, PdfFont header, PdfFont body) GetTemplateFonts(string templateStyle)
        {
            var titleFont = templateStyle switch
            {
                "modern" => PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD),
                "minimal" => PdfFontFactory.CreateFont(StandardFonts.HELVETICA),
                _ => PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD)
            };

            var headerFont = templateStyle switch
            {
                "modern" => PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD),
                "minimal" => PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD),
                _ => PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD)
            };

            var bodyFont = templateStyle switch
            {
                "modern" => PdfFontFactory.CreateFont(StandardFonts.HELVETICA),
                "minimal" => PdfFontFactory.CreateFont(StandardFonts.HELVETICA),
                _ => PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN)
            };

            return (titleFont, headerFont, bodyFont);
        }

        private void AddHeader(Document document, Resume resume, string templateStyle, Color primaryColor, PdfFont titleFont)
        {
            var title = new Paragraph(resume.Title)
                .SetFont(titleFont)
                .SetFontSize(templateStyle == "minimal" ? 24 : 22)
                .SetFontColor(primaryColor)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);

            if (templateStyle == "modern")
            {
                // Add a decorative line under the title
                var line = new Paragraph("")
                    .SetHeight(2)
                    .SetBackgroundColor(primaryColor)
                    .SetMarginBottom(20);
                document.Add(title);
                document.Add(line);
            }
            else
            {
                document.Add(title);
            }
        }

        private void AddClassicLayout(Document document, Resume resume, Color primaryColor, PdfFont headerFont, PdfFont bodyFont)
        {
            AddSection(document, "Personal Information", resume.PersonalInfo, primaryColor, headerFont, bodyFont, 16);
            AddSection(document, "Education", resume.Education, primaryColor, headerFont, bodyFont, 16);
            AddSection(document, "Experience", resume.Experience, primaryColor, headerFont, bodyFont, 16);
            AddSection(document, "Skills", resume.Skills, primaryColor, headerFont, bodyFont, 16);
        }

        private void AddMinimalLayout(Document document, Resume resume, Color primaryColor, PdfFont headerFont, PdfFont bodyFont)
        {
            // Minimal layout with more whitespace and clean typography
            AddSection(document, "About", resume.PersonalInfo, primaryColor, headerFont, bodyFont, 18);
            AddSection(document, "Education", resume.Education, primaryColor, headerFont, bodyFont, 18);
            AddSection(document, "Experience", resume.Experience, primaryColor, headerFont, bodyFont, 18);
            AddSection(document, "Skills", resume.Skills, primaryColor, headerFont, bodyFont, 18);
        }

        private void AddModernLayout(Document document, Resume resume, Color primaryColor, Color secondaryColor, PdfFont headerFont, PdfFont bodyFont)
        {
            // Modern layout with side-by-side sections and visual hierarchy
            var table = new Table(2).UseAllAvailableWidth();
            
            // Left column
            var leftCell = new Cell().SetBorder(null).SetPadding(10);
            leftCell.Add(new Paragraph("About").SetFont(headerFont).SetFontSize(16).SetFontColor(primaryColor));
            if (!string.IsNullOrWhiteSpace(resume.PersonalInfo))
                leftCell.Add(new Paragraph(resume.PersonalInfo).SetFont(bodyFont).SetFontSize(10));
            
            leftCell.Add(new Paragraph("Skills").SetFont(headerFont).SetFontSize(16).SetFontColor(primaryColor).SetMarginTop(20));
            if (!string.IsNullOrWhiteSpace(resume.Skills))
                leftCell.Add(new Paragraph(resume.Skills).SetFont(bodyFont).SetFontSize(10));
            
            // Right column
            var rightCell = new Cell().SetBorder(null).SetPadding(10);
            rightCell.Add(new Paragraph("Experience").SetFont(headerFont).SetFontSize(16).SetFontColor(primaryColor));
            if (!string.IsNullOrWhiteSpace(resume.Experience))
                rightCell.Add(new Paragraph(resume.Experience).SetFont(bodyFont).SetFontSize(10));
            
            rightCell.Add(new Paragraph("Education").SetFont(headerFont).SetFontSize(16).SetFontColor(primaryColor).SetMarginTop(20));
            if (!string.IsNullOrWhiteSpace(resume.Education))
                rightCell.Add(new Paragraph(resume.Education).SetFont(bodyFont).SetFontSize(10));
            
            table.AddCell(leftCell);
            table.AddCell(rightCell);
            document.Add(table);
        }

        private void AddSection(Document document, string header, string? content, Color primaryColor, PdfFont headerFont, PdfFont bodyFont, float headerSize)
        {
            if (string.IsNullOrWhiteSpace(content)) return;
            
            var headerPara = new Paragraph(header)
                .SetFont(headerFont)
                .SetFontSize(headerSize)
                .SetFontColor(primaryColor)
                .SetMarginTop(20)
                .SetMarginBottom(10);
            document.Add(headerPara);
            
            var contentPara = new Paragraph(content.Trim())
                .SetFont(bodyFont)
                .SetFontSize(11)
                .SetMarginBottom(15);
            document.Add(contentPara);
        }
    }
}