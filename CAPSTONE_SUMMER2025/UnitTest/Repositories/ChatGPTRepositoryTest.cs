using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using API.DTO.PolicyDTO;
using API.Repositories;
using API.Repositories.Interfaces;
using API.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace UnitTest.Repositories
{
    public class ChatGPTRepositoryTest
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IFormFile> _formFileMock;
        private readonly IChatGPTRepository _repository;

        public ChatGPTRepositoryTest()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["OpenAI:ApiKey"]).Returns("fake-api-key");
            _configurationMock.Setup(c => c["OpenAI:SystemPrompt"]).Returns("You are a policy checker.");
            _formFileMock = new Mock<IFormFile>();
            _repository = new ChatGPTRepository(_httpClient, _configurationMock.Object);
        }

        #region CheckPostPolicyAsync
        [Fact]
        public async Task CheckPostPolicyAsync_ValidInput_ReturnsResponse()
        {
            // Arrange
            var postContent = "This is a valid post.";
            var policies = new List<resPolicyDTO> { new resPolicyDTO { Description = "No inappropriate content." } };
            var responseContent = @"{""choices"": [{""message"": {""content"": ""Valid""}}]}";
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _repository.CheckPostPolicyAsync(postContent, policies);

            // Assert
            Assert.Equal("Valid", result);
        }

        [Fact]
        public async Task CheckPostPolicyAsync_HttpError_ThrowsHttpRequestException()
        {
            // Arrange
            var postContent = "This is a post.";
            var policies = new List<resPolicyDTO> { new resPolicyDTO { Description = "No inappropriate content." } };
            var responseContent = @"{""error"": ""Bad request""}";
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _repository.CheckPostPolicyAsync(postContent, policies));
        }

        [Fact]
        public async Task CheckPostPolicyAsync_EmptyPostContent_ReturnsEmptyResponse()
        {
            // Arrange
            string postContent = "";
            var policies = new List<resPolicyDTO> { new resPolicyDTO { Description = "No inappropriate content." } };
            var responseContent = @"{""choices"": [{""message"": {""content"": ""Invalid""}}]}";
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });
            // Act
            var result = await _repository.CheckPostPolicyAsync(postContent, policies);
            // Assert
            Assert.Equal("Invalid", result);
        }

        [Fact]
        public async Task CheckPostPolicyAsync_NullPostContent_ReturnsEmptyResponse()
        {
            // Arrange
            string postContent = null; 
            var policies = new List<resPolicyDTO> { new resPolicyDTO { Description = "No inappropriate content." } };
            var responseContent = @"{""choices"": [{""message"": {""content"": ""Invalid""}}]}";
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });
            // Act
            var result = await _repository.CheckPostPolicyAsync(postContent, policies);
            // Assert
            Assert.Equal("Invalid", result); 
        }
        #endregion

        #region CheckPostPolicyWithUploadedImageAsync
        [Fact]
        public async Task CheckPostPolicyWithUploadedImageAsync_ValidInput_ReturnsValidResponse()
        {
            // Arrange
            var postContent = "This is a post with image.";
            var policies = new List<resPolicyDTO> { new resPolicyDTO { Description = "No inappropriate images." } };
            var fileContent = Encoding.UTF8.GetBytes("fake-image-data");
            var memoryStream = new MemoryStream(fileContent);
            _formFileMock.Setup(f => f.FileName).Returns("test.jpg");
            _formFileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            _formFileMock.Setup(f => f.Length).Returns(fileContent.Length);
            _formFileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            _formFileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, ct) => memoryStream.CopyTo(stream))
                .Returns(Task.CompletedTask);
            var responseContent = @"{""choices"": [{""message"": {""content"": ""Valid""}}]}";
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _repository.CheckPostPolicyWithUploadedImageAsync(postContent, _formFileMock.Object, policies);

            // Assert
            Assert.Equal("Valid", result);
        }

        [Fact]
        public async Task CheckPostPolicyWithUploadedImageAsync_HttpError_ThrowsHttpRequestException()
        {
            // Arrange
            var postContent = "This is a post with image.";
            var policies = new List<resPolicyDTO> { new resPolicyDTO { Description = "No inappropriate images." } };
            var fileContent = Encoding.UTF8.GetBytes("fake-image-data");
            var memoryStream = new MemoryStream(fileContent);
            _formFileMock.Setup(f => f.FileName).Returns("test.jpg");
            _formFileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            _formFileMock.Setup(f => f.Length).Returns(fileContent.Length);
            _formFileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            _formFileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, ct) => memoryStream.CopyTo(stream))
                .Returns(Task.CompletedTask);
            var responseContent = @"{""error"": ""Bad request""}";
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _repository.CheckPostPolicyWithUploadedImageAsync(postContent, _formFileMock.Object, policies));
        }

        [Fact]
        public async Task CheckPostPolicyWithUploadedImageAsync_EmptyPostContent_ReturnsValidResponse()
        {
            // Arrange
            var postContent = "";
            var policies = new List<resPolicyDTO> { new resPolicyDTO { Description = "No inappropriate images." } };
            var fileContent = Encoding.UTF8.GetBytes("fake-image-data");
            var memoryStream = new MemoryStream(fileContent);
            _formFileMock.Setup(f => f.FileName).Returns("image.png");
            _formFileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            _formFileMock.Setup(f => f.Length).Returns(fileContent.Length);
            _formFileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            _formFileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, ct) => memoryStream.CopyTo(stream))
                .Returns(Task.CompletedTask);
            var responseContent = @"{""choices"": [{""message"": {""content"": ""Valid""}}]}";
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });
            // Act
            var result = await _repository.CheckPostPolicyWithUploadedImageAsync(postContent, _formFileMock.Object, policies);
            // Assert
            Assert.Equal("Valid", result);
        }

        [Fact]
        public async Task CheckPostPolicyWithUploadedImageAsync_NullPostContent_ReturnsValidResponse()
        {
            // Arrange
            string postContent = null;
            var policies = new List<resPolicyDTO> { new resPolicyDTO { Description = "No inappropriate images." } };
            var fileContent = Encoding.UTF8.GetBytes("fake-image-data");
            var memoryStream = new MemoryStream(fileContent);
            _formFileMock.Setup(f => f.FileName).Returns("image.png");
            _formFileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            _formFileMock.Setup(f => f.Length).Returns(fileContent.Length);
            _formFileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            _formFileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, ct) => memoryStream.CopyTo(stream))
                .Returns(Task.CompletedTask);
            var responseContent = @"{""choices"": [{""message"": {""content"": ""Valid""}}]}";
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });
            // Act
            var result = await _repository.CheckPostPolicyWithUploadedImageAsync(postContent, _formFileMock.Object, policies);
            // Assert
            Assert.Equal("Valid", result);
        }
        [Fact]
        public async Task CheckPostPolicyWithUploadedImageAsync_EmptyImageFile_ReturnsInvalidResponse()
        {
            // Arrange
            var postContent = "This is a valid post.";
            var policies = new List<resPolicyDTO> { new resPolicyDTO { Description = "No inappropriate images." } };
            var fileContent = Array.Empty<byte>(); 
            var memoryStream = new MemoryStream(fileContent);
            _formFileMock.Setup(f => f.FileName).Returns("image.png");
            _formFileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            _formFileMock.Setup(f => f.Length).Returns(fileContent.Length);
            _formFileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            _formFileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, ct) => memoryStream.CopyTo(stream))
                .Returns(Task.CompletedTask);
            var responseContent = @"{""choices"": [{""message"": {""content"": ""Invalid""}}]}";
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });
            // Act
            var result = await _repository.CheckPostPolicyWithUploadedImageAsync(postContent, _formFileMock.Object, policies);
            // Assert
            Assert.Equal("Invalid", result);
        }

        [Fact]
        public async Task CheckPostPolicyWithUploadedImageAsync_ViolatingImageFile_ReturnsInvalidResponse()
        {
            // Arrange
            var postContent = "This is a valid post.";
            var policies = new List<resPolicyDTO> { new resPolicyDTO { Description = "No inappropriate images." } };
            var fileContent = Encoding.UTF8.GetBytes("inappropriate-image-data");
            var memoryStream = new MemoryStream(fileContent);
            _formFileMock.Setup(f => f.FileName).Returns("image.png");
            _formFileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            _formFileMock.Setup(f => f.Length).Returns(fileContent.Length);
            _formFileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            _formFileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, ct) => memoryStream.CopyTo(stream))
                .Returns(Task.CompletedTask);
            var responseContent = @"{""choices"": [{""message"": {""content"": ""Invalid: Inappropriate content detected""}}]}";
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });
            // Act
            var result = await _repository.CheckPostPolicyWithUploadedImageAsync(postContent, _formFileMock.Object, policies);
            // Assert
            Assert.Equal("Invalid: Inappropriate content detected", result);
        }
        #endregion

        #region EvaluateCVAgainstPositionAsync
        [Fact]
        public async Task EvaluateCVAgainstPositionAsync_ValidInput_ReturnsEvaluationResult()
        {
            // Arrange
            var cvText = "Experienced developer with 5 years in tech.";
            var positionDescription = "Senior Developer position.";
            var positionRequirement = "5+ years experience, strong coding skills.";
            var responseContent = @"{""choices"": [{""message"": {""content"": ""{\""Evaluation_TechSkills\"": {\""Score\"": 8, \""Comment\"": \""Good skills\""}, \""Evaluation_Experience\"": {\""Score\"": 7, \""Comment\"": \""Relevant experience\""}, \""Evaluation_SoftSkills\"": {\""Score\"": 6, \""Comment\"": \""Average soft skills\""}, \""Evaluation_OverallSummary\"": {\""Score\"": 7, \""Comment\"": \""Good candidate\""}}""}}]}";
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _repository.EvaluateCVAgainstPositionAsync(cvText, positionDescription, positionRequirement);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("[8] Good skills", result.Evaluation_TechSkills);
            Assert.Equal("[7] Relevant experience", result.Evaluation_Experience);
            Assert.Equal("[6] Average soft skills", result.Evaluation_SoftSkills);
            Assert.Equal("[7] Good candidate", result.Evaluation_OverallSummary);
        }

        [Fact]
        public async Task EvaluateCVAgainstPositionAsync_EmptyCVText_ReturnsEvaluationResult()
        {
            // Arrange
            var cvText = "";
            var positionDescription = "Senior Developer position.";
            var positionRequirement = "5+ years experience, strong coding skills.";
            var responseContent = @"{""choices"": [{""message"": {""content"": ""{\""Evaluation_TechSkills\"": {\""Score\"": 0, \""Comment\"": \""No technical skills provided\""}, \""Evaluation_Experience\"": {\""Score\"": 0, \""Comment\"": \""No experience provided\""}, \""Evaluation_SoftSkills\"": {\""Score\"": 0, \""Comment\"": \""No soft skills provided\""}, \""Evaluation_OverallSummary\"": {\""Score\"": 0, \""Comment\"": \""Missing CV content\""}}""}}]}";
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });
            // Act
            var result = await _repository.EvaluateCVAgainstPositionAsync(cvText, positionDescription, positionRequirement);
            // Assert
            Assert.NotNull(result);
            Assert.Equal("[0] No technical skills provided", result.Evaluation_TechSkills);
            Assert.Equal("[0] No experience provided", result.Evaluation_Experience);
            Assert.Equal("[0] No soft skills provided", result.Evaluation_SoftSkills);
            Assert.Equal("[0] Missing CV content", result.Evaluation_OverallSummary);
        }

        [Fact]
        public async Task EvaluateCVAgainstPositionAsync_NullCVText_ReturnsEvaluationResult()
        {
            // Arrange
            string cvText = null;
            var positionDescription = "Senior Developer position.";
            var positionRequirement = "5+ years experience, strong coding skills.";
            var responseContent = @"{""choices"": [{""message"": {""content"": ""{\""Evaluation_TechSkills\"": {\""Score\"": 0, \""Comment\"": \""No technical skills provided\""}, \""Evaluation_Experience\"": {\""Score\"": 0, \""Comment\"": \""No experience provided\""}, \""Evaluation_SoftSkills\"": {\""Score\"": 0, \""Comment\"": \""No soft skills provided\""}, \""Evaluation_OverallSummary\"": {\""Score\"": 0, \""Comment\"": \""Missing CV content\""}}""}}]}";
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });
            // Act
            var result = await _repository.EvaluateCVAgainstPositionAsync(cvText, positionDescription, positionRequirement);
            // Assert
            Assert.NotNull(result);
            Assert.Equal("[0] No technical skills provided", result.Evaluation_TechSkills);
            Assert.Equal("[0] No experience provided", result.Evaluation_Experience);
            Assert.Equal("[0] No soft skills provided", result.Evaluation_SoftSkills);
            Assert.Equal("[0] Missing CV content", result.Evaluation_OverallSummary);
        }

        [Fact]
        public async Task EvaluateCVAgainstPositionAsync_HttpError_ThrowsHttpRequestException()
        {
            // Arrange
            var cvText = "CV: Experienced developer.";
            var positionDescription = "Looking for a developer.";
            var positionRequirement = "Experience required.";
            var responseContent = @"{""error"": ""Bad request""}";
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    Console.WriteLine($"SendAsync called with RequestUri: {req.RequestUri}, CancellationToken: {ct == CancellationToken.None}");
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _repository.EvaluateCVAgainstPositionAsync(cvText, positionDescription, positionRequirement));
            Assert.Contains("OpenAI API lỗi 400", exception.Message);
        }
        #endregion
    }
}