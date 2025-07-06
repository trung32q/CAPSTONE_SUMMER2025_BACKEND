﻿namespace API.Repositories.Interfaces
{
    public interface IFilebaseHandler
    {
        Task<string> UploadMediaFile(IFormFile file);
        string GeneratePreSignedUrl(string fileName);
        Task<bool> DeleteFileByUrlAsync(string fileUrl);
        string GenerateSignedRawUrl(string publicId, TimeSpan validDuration);
        Task<string> UploadAuthenticatedRawFile(IFormFile file);
        Task<string> UploadPdfAsync(IFormFile file);
        string GeneratePresignedPDFUrl(string key, int expireHours = 2);
    }
}
