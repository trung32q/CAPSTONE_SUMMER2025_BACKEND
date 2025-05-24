namespace API.Repositories.Interfaces
{
    public interface IFilebaseHandler
    {
        Task<string> UploadMediaFile(IFormFile file);
    }
}
