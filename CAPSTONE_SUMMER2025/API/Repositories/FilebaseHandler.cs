using Amazon;
using Amazon.S3.Transfer;
using Amazon.S3;
using API.Repositories.Interfaces;
using AutoMapper;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class FilebaseHandler : IFilebaseHandler
    {
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _bucketName;
        private readonly RegionEndpoint _region;
        private readonly string _serviceUrl;

        public FilebaseHandler(IConfiguration configuration)
        {
            // Load values from appsettings.json
            _accessKey = configuration["AWS:AccessKey"];
            _secretKey = configuration["AWS:SecretKey"];
            _bucketName = configuration["AWS:BucketName"];
            _region = RegionEndpoint.GetBySystemName(configuration["AWS:Region"]);
            _serviceUrl = configuration["AWS:ServiceURL"];
            
        }

        //hàm xử lí upload media file rồi trả về đường dẫn
        public async Task<string> UploadMediaFile(IFormFile file)
        {
            var config = new AmazonS3Config
            {
                RegionEndpoint = _region,
                ServiceURL = _serviceUrl,
                ForcePathStyle = true
            };

            using var client = new AmazonS3Client(_accessKey, _secretKey, config);
            using var newMemoryStream = new MemoryStream();
            await file.CopyToAsync(newMemoryStream);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = newMemoryStream,
                Key = file.FileName,
                BucketName = _bucketName,
                ContentType = file.ContentType
            };

            var fileTransferUtility = new TransferUtility(client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            var fileUrl = $"https://{_bucketName}.s3.filebase.com/{file.FileName}";
            return fileUrl;
        }

    }
}
