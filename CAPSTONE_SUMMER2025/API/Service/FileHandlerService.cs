using API.Repositories.Interfaces;
using API.Service.Interface;

namespace API.Service
{
    public class FileHandlerService : IFileHandlerService
    {
        private readonly IFileHandlerRepository _fileHandlerRepository;

        public FileHandlerService(IFileHandlerRepository fileHandlerRepository)
        {
            _fileHandlerRepository = fileHandlerRepository;
        }
        public async Task<string> GetTextFromPdfAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new InvalidDataException("File không hợp lệ.");

            if (!file.FileName.EndsWith(".pdf"))
                throw new InvalidDataException("Chỉ hỗ trợ file PDF.");

            using var stream = file.OpenReadStream();
            var text = await _fileHandlerRepository.ExtractTextFromPdfAsync(stream);
            return text;

        }
    }
}
