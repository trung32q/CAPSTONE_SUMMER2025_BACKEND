namespace API.Service.Interface
{
    public interface IFileHandlerService
    {
        Task<string> GetTextFromPdfAsync(IFormFile file);
    }
}
