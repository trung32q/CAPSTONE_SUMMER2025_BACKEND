using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using API.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace API.Repositories
{
    public class FilebaseHandler : IFilebaseHandler
    {
        private readonly Cloudinary _cloudinary;

        public FilebaseHandler(IConfiguration configuration)
        {
            // Load values from appsettings.json
            var cloudName = configuration["CloudinarySettings:CloudName"];
            var apiKey = configuration["CloudinarySettings:ApiKey"];
            var apiSecret = configuration["CloudinarySettings:ApiSecret"];

            // Validate configuration
            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
                throw new Exception("Cloudinary configuration is missing or invalid");

            // Initialize Cloudinary with fully qualified CloudinaryDotNet.Account
            var account = new CloudinaryDotNet.Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadMediaFile(IFormFile file)
        {
            // Check if file is null or empty
            if (file == null || file.Length == 0)
                throw new Exception("File rỗng hoặc không tồn tại");

            // Generate unique file name
            var fileName = file.FileName ?? "unnamed_file";
            var extension = Path.GetExtension(fileName)?.ToLower() ?? "";
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            // Log the file details for debugging
            Console.WriteLine($"File Name: {fileName}");
            Console.WriteLine($"File ContentType: {file.ContentType}");
            Console.WriteLine($"File Extension: {extension}");

            // Determine resource type (image, video, or raw)
            var resourceType = file.ContentType switch
            {
                var type when type.StartsWith("image/") => "image",
                var type when type.StartsWith("video/") => "video",
                var type when type == "application/pdf" => "raw",
                _ => extension switch // Fallback to extension-based detection
                {
                    ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" => "image",
                    ".mp4" or ".mov" or ".avi" or ".webm" => "video",
                    ".pdf" => "raw",
                    _ => "raw" // Default for other file types
                }
            };

            // Log the determined resourceType for debugging
            Console.WriteLine($"Determined resourceType: {resourceType}");

            // Upload parameters based on resource type
            UploadResult uploadResult;
            switch (resourceType)
            {
                case "image":
                    var imageParams = new ImageUploadParams
                    {
                        File = new FileDescription(uniqueFileName, file.OpenReadStream()),
                        Folder = "media" // Organize files in the "media" folder
                    };
                    uploadResult = await _cloudinary.UploadAsync(imageParams);
                    break;

                case "video":
                    var videoParams = new VideoUploadParams
                    {
                        File = new FileDescription(uniqueFileName, file.OpenReadStream()),
                        Folder = "media"
                    };
                    uploadResult = await _cloudinary.UploadAsync(videoParams);
                    break;

                case "raw":
                default:
                    var rawParams = new RawUploadParams
                    {
                        File = new FileDescription(uniqueFileName, file.OpenReadStream()),
                        Folder = "media"
                    };
                    uploadResult = await _cloudinary.UploadAsync(rawParams);
                    break;
            }

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception($"Upload failed: {uploadResult.Error?.Message}");

            // Log the upload result
            Console.WriteLine($"Upload successful. PublicId: {uploadResult.PublicId}");
            Console.WriteLine($"Generated URL: {uploadResult.SecureUrl}");

            // Return the secure URL of the uploaded file
            return uploadResult.SecureUrl.ToString();
        }

        public async Task<bool> DeleteFileByUrlAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                throw new ArgumentException("URL không hợp lệ.");

            try
            {
                var uri = new Uri(fileUrl);
                var segments = uri.AbsolutePath.Split('/');
                var uploadIndex = Array.IndexOf(segments, "upload");
                if (uploadIndex < 0 || uploadIndex >= segments.Length - 1)
                    throw new Exception("Không tìm thấy publicId trong URL.");

                var publicIdParts = segments.Skip(uploadIndex + 1).ToList();

                // Bỏ "v123..." nếu có
                if (publicIdParts[0].StartsWith("v") && long.TryParse(publicIdParts[0].Substring(1), out _))
                {
                    publicIdParts.RemoveAt(0);
                }

                // Bỏ phần mở rộng
                var lastPart = publicIdParts.Last();
                var lastPartWithoutExt = Path.GetFileNameWithoutExtension(lastPart);
                publicIdParts[publicIdParts.Count - 1] = lastPartWithoutExt;

                var publicId = string.Join("/", publicIdParts);

                Console.WriteLine($"[DEBUG] publicId: {publicId}");

                var deletionParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Auto,
                    Invalidate = true 

                };

                var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

                Console.WriteLine($"[DEBUG] Deletion result: {deletionResult.Result}");

                return deletionResult.Result == "ok";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] DeleteFileByUrlAsync: {ex.Message}");
                return false;
            }
        }

        public string GeneratePreSignedUrl(string mediaUrlFromDb)
        {
            if (string.IsNullOrEmpty(mediaUrlFromDb))
                throw new Exception("mediaUrlFromDb cannot be null or empty");

            // Nếu đã là URL đầy đủ và là video thì chèn f_mp4 nếu cần
            if (mediaUrlFromDb.StartsWith("https://"))
            {
                // Nếu là video Cloudinary thì xử lý f_mp4
                if (mediaUrlFromDb.Contains("/video/") && mediaUrlFromDb.Contains("/upload/") && !mediaUrlFromDb.Contains("/f_mp4/"))
                {
                    // Chèn f_mp4/ sau upload/
                    var idx = mediaUrlFromDb.IndexOf("/upload/") + "/upload/".Length;
                    var urlWithMp4 = mediaUrlFromDb.Insert(idx, "f_mp4/");
                    return urlWithMp4;
                }
                // Còn lại trả về nguyên bản
                return mediaUrlFromDb;
            }

            string cloudName = "dbtrnadoo";

            // Nếu lưu từ DB là path (không phải url đầy đủ)
            if (mediaUrlFromDb.Contains("video/upload/"))
            {
                string videoPath = mediaUrlFromDb.Insert(mediaUrlFromDb.IndexOf("video/upload/") + "video/upload/".Length, "f_mp4/");
                return $"https://res.cloudinary.com/{cloudName}/{videoPath}";
            }
            else if (mediaUrlFromDb.Contains("image/upload/"))
            {
                return $"https://res.cloudinary.com/{cloudName}/{mediaUrlFromDb}";
            }
            else if (mediaUrlFromDb.Contains("raw/upload/"))
            {
                return $"https://res.cloudinary.com/{cloudName}/{mediaUrlFromDb}";
            }
            else
            {
                return $"https://res.cloudinary.com/{cloudName}/{mediaUrlFromDb}";
            }
        }




    }
}