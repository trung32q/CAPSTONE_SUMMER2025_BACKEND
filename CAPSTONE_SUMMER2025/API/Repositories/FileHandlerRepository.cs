using System.Text;
using API.Repositories.Interfaces;
using UglyToad.PdfPig;

namespace API.Repositories
{
    public class FileHandlerRepository : IFileHandlerRepository
    {
        public async Task<string> ExtractTextFromPdfAsync(Stream pdfStream)
        {
            using var pdf = PdfDocument.Open(pdfStream);
            var sb = new StringBuilder();

            foreach (var page in pdf.GetPages())
            {
                sb.AppendLine(page.Text);
            }

            return sb.ToString();
        }
    }
}
