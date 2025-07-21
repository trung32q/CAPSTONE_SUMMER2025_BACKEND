using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using API.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3.Transfer;
using Amazon.S3;
using System.Text;
using Amazon.S3.Model;

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


        public async Task<string> UploadAuthenticatedRawFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File rỗng hoặc không tồn tại");

            // Tạo tên file duy nhất
            var extension = Path.GetExtension(file.FileName)?.ToLower();
            if (string.IsNullOrWhiteSpace(extension))
                extension = ".bin";

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            Console.WriteLine("[UPLOAD AUTHENTICATED] File: " + file.FileName);
            Console.WriteLine("[UPLOAD AUTHENTICATED] Extension: " + extension);

            // Upload với chế độ authenticated
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(uniqueFileName, file.OpenReadStream()),
                Folder = "media",
                Type = "authenticated" // Quan trọng!
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception($"Upload failed: {uploadResult.Error?.Message}");

            Console.WriteLine("[UPLOAD AUTHENTICATED] PublicId: " + uploadResult.PublicId);
            Console.WriteLine("[UPLOAD AUTHENTICATED] SecureUrl: " + uploadResult.SecureUrl);

            return uploadResult.SecureUrl?.ToString();
        }


        public string GenerateSignedRawUrl(string originalUrl, TimeSpan validDuration)
        {
            if (string.IsNullOrWhiteSpace(originalUrl))
                throw new ArgumentException("URL không hợp lệ.");

            var uri = new Uri(originalUrl);
            var segments = uri.AbsolutePath.Split('/');

            // Tìm index của "upload" hoặc "authenticated"
            var uploadIndex = Array.IndexOf(segments, "upload");
            var authIndex = Array.IndexOf(segments, "authenticated");

            int baseIndex = -1;
            string resourceType;

            if (uploadIndex >= 0)
            {
                baseIndex = uploadIndex;
                resourceType = "raw";
            }
            else if (authIndex >= 0)
            {
                baseIndex = authIndex;
                resourceType = "raw";
            }
            else
            {
                throw new Exception($"URL không chứa 'upload' hoặc 'authenticated': {originalUrl}");
            }

            // Skip thêm nếu có s--token-- sau authenticated
            var parts = segments.Skip(baseIndex + 1).ToList();

            if (parts.Count > 0 && parts[0].StartsWith("s--") && parts[0].EndsWith("--"))
            {
                // Bỏ token
                parts.RemoveAt(0);
            }

            // Bỏ version
            if (parts.Count > 0 && parts[0].StartsWith("v") && long.TryParse(parts[0].Substring(1), out _))
            {
                parts.RemoveAt(0);
            }

            if (parts.Count == 0)
                throw new Exception($"URL không chứa public_id hợp lệ: {originalUrl}");

            var lastPart = parts.Last();
            var filenameNoExt = Path.GetFileNameWithoutExtension(lastPart);
            parts[parts.Count - 1] = filenameNoExt;

            var publicId = string.Join("/", parts);

            var expiration = DateTimeOffset.UtcNow.Add(validDuration).ToUnixTimeSeconds();

            Console.WriteLine("OriginalUrl: " + originalUrl);
            Console.WriteLine("PublicId: " + publicId);
            Console.WriteLine("Expiration: " + expiration);

            var parametersToSign = new SortedDictionary<string, object>
    {
        { "public_id", publicId },
        { "resource_type", "raw" },
        { "timestamp", expiration.ToString() }
    };

            var signature = _cloudinary.Api.SignParameters(parametersToSign);

            var signedUrl = $"https://res.cloudinary.com/{_cloudinary.Api.Account.Cloud}/raw/upload/ts_{expiration},sig_{signature}/{publicId}.pdf";

            return signedUrl;
        }


        //upload rồi trả về key
        public async Task<string> UploadPdfAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            var config = new AmazonS3Config
            {
                ServiceURL = "https://s3.filebase.com",
                ForcePathStyle = true
            };

            // Tạo tên file ngẫu nhiên giữ nguyên đuôi mở rộng
            var extension = Path.GetExtension(file.FileName);
            var randomName = $"{Guid.NewGuid():N}{extension}";

            using (var client = new AmazonS3Client(
                "185323F8105191E3009D",
                "MXoHSlXNV2UrElhlUVDUhpCFS0OVNsJlUebrZyC6",
                config))
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadRequest = new Amazon.S3.Model.PutObjectRequest
                    {
                        BucketName = "media-file",
                        Key = randomName,
                        InputStream = stream,
                        ContentType = file.ContentType ?? "application/pdf"
                        // KHÔNG đặt CannedACL
                    };

                    await client.PutObjectAsync(uploadRequest);
                }

                // Trả về key lưu trong database
                return randomName;
            }
        }

        //lấy ra pre link của filebase
        public string GeneratePresignedPDFUrl(string key, int expireHours = 2)
        {
            var config = new AmazonS3Config
            {
                ServiceURL = "https://s3.filebase.com",
                ForcePathStyle = true
            };

            using (var client = new AmazonS3Client("185323F8105191E3009D",
                "MXoHSlXNV2UrElhlUVDUhpCFS0OVNsJlUebrZyC6", config))
            {
                var urlRequest = new Amazon.S3.Model.GetPreSignedUrlRequest
                {
                    BucketName = "media-file",
                    Key = key,
                    Expires = DateTime.UtcNow.AddHours(expireHours)
                };

                return client.GetPreSignedURL(urlRequest);
            }
        }

        //xóa file trên filebase
        public async Task DeleteFileOnFilebaseAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key không hợp lệ.");

            var config = new AmazonS3Config
            {
                ServiceURL = "https://s3.filebase.com",
                ForcePathStyle = true
            };

            using (var client = new AmazonS3Client(
                "185323F8105191E3009D",
                "MXoHSlXNV2UrElhlUVDUhpCFS0OVNsJlUebrZyC6",
                config))
            {
                var deleteRequest = new Amazon.S3.Model.DeleteObjectRequest
                {
                    BucketName = "media-file",
                    Key = key
                };

                await client.DeleteObjectAsync(deleteRequest);
            }
        }


        private const string BackblazeServiceUrl = "https://s3.us-east-005.backblazeb2.com";
        private const string BackblazeBucketName = "simes-media-file";
        private const string BackblazeAccessKey = "005891643634b660000000002";
        private const string BackblazeSecretKey = "K005Qd8vlO+4zIez3Z/1nHDhqHFy2c8";


        public async Task<string> UploadPdfToBackblazeAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            var extension = Path.GetExtension(file.FileName);
            var randomKey = $"{Guid.NewGuid():N}{extension}";

            var config = new AmazonS3Config
            {
                ServiceURL = BackblazeServiceUrl,
                ForcePathStyle = true
            };

            using var client = new AmazonS3Client(BackblazeAccessKey, BackblazeSecretKey, config);
            using var stream = file.OpenReadStream();

            var request = new PutObjectRequest
            {
                BucketName = BackblazeBucketName,
                Key = randomKey,
                InputStream = stream,
                ContentType = file.ContentType ?? "application/octet-stream"
            };

            await client.PutObjectAsync(request);
            return randomKey;
        }

        public string GeneratePresignedBackblazePDFUrl(string key, int expireHours = 2)
        {
            var config = new AmazonS3Config
            {
                ServiceURL = BackblazeServiceUrl,
                ForcePathStyle = true
            };

            using var client = new AmazonS3Client(BackblazeAccessKey, BackblazeSecretKey, config);

            var urlRequest = new GetPreSignedUrlRequest
            {
                BucketName = BackblazeBucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddHours(expireHours)
            };

            return client.GetPreSignedURL(urlRequest);
        }

        public async Task DeleteFileOnBackblazeAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key không hợp lệ.");

            var config = new AmazonS3Config
            {
                ServiceURL = BackblazeServiceUrl,
                ForcePathStyle = true
            };

            using var client = new AmazonS3Client(BackblazeAccessKey, BackblazeSecretKey, config);

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = BackblazeBucketName,
                Key = key
            };

            await client.DeleteObjectAsync(deleteRequest);
        }

    }

}
