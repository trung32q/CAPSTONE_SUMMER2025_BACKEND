using Amazon;
using Amazon.S3.Transfer;
using Amazon.S3;
using API.Repositories.Interfaces;
using AutoMapper;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Amazon.S3.Model;

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

        public async Task<string> UploadMediaFile(IFormFile file)
        {
            var config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.USEast1,
                ServiceURL = "https://s3.filebase.com",
                ForcePathStyle = true
            };

            using var client = new AmazonS3Client(_accessKey, _secretKey, config);
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0; // ⚠️ Bắt buộc

            var extension = Path.GetExtension(file.FileName); // giữ đúng loại file
            var fileName = $"{Guid.NewGuid()}{extension}";

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = fileName,
                BucketName = _bucketName,
                ContentType = file.ContentType // ⚠️ Bắt buộc
            };

            var transferUtility = new TransferUtility(client);
            await transferUtility.UploadAsync(uploadRequest);

            return fileName;
        }



        public string GeneratePreSignedUrl(string fileName)
        {
            var credentials = new Amazon.Runtime.BasicAWSCredentials(_accessKey, _secretKey);
            var config = new AmazonS3Config
            {
                RegionEndpoint = _region,
                ServiceURL = _serviceUrl,
                ForcePathStyle = true
            };

            using var client = new AmazonS3Client(credentials, config);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = "media-file",
                Key = fileName,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };

            return client.GetPreSignedURL(request);
        }
    }
}
