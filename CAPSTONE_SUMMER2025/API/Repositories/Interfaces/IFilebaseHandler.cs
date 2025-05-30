namespace API.Repositories.Interfaces
{
    public interface IFilebaseHandler
    {
        Task<string> UploadMediaFile(IFormFile file);
        string GeneratePreSignedUrl(string fileName);
        Task<bool> DeleteFileByUrlAsync(string fileUrl);
    }
}
