namespace API.Repositories.Interfaces
{
    public interface IFileHandlerRepository
    {
        Task<string> ExtractTextFromPdfAsync(Stream pdfStream);
    }
}
