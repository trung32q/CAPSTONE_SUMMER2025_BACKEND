using API.Repositories;
using API.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTest.Repositories
{
    public class FptAIRepositoryTest
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IFormFile> _cccdImageMock;
        private readonly Mock<IFormFile> _selfieImageMock;
        private readonly FptAIRepository _fptAIRepository;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;

        public FptAIRepositoryTest()
        {
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["FPTAI:ApiKey"]).Returns("test-api-key");
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpMessageHandlerMock
                .Protected()
                .Setup("Dispose", ItExpr.IsAny<bool>())
                .Verifiable();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(_httpClient);
            _fptAIRepository = new FptAIRepository(_httpClientFactoryMock.Object, _configurationMock.Object);
            _cccdImageMock = new Mock<IFormFile>();
            _selfieImageMock = new Mock<IFormFile>();

            var cccdStream = new MemoryStream(new byte[] { 1, 2, 3 });
            var selfieStream = new MemoryStream(new byte[] { 4, 5, 6 });

            _cccdImageMock.Setup(f => f.FileName).Returns("cccd.jpg");
            _cccdImageMock.Setup(f => f.Length).Returns(cccdStream.Length);
            _cccdImageMock.Setup(f => f.ContentType).Returns("image/jpeg");
            _cccdImageMock.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(new byte[] { 1, 2, 3 }));

            _selfieImageMock.Setup(f => f.FileName).Returns("selfie.jpg");
            _selfieImageMock.Setup(f => f.Length).Returns(selfieStream.Length);
            _selfieImageMock.Setup(f => f.ContentType).Returns("image/jpeg");
            _selfieImageMock.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(new byte[] { 4, 5, 6 }));

            cccdStream.Dispose();
            selfieStream.Dispose();
        }

        private HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string content, string contentType = "application/json")
        {
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, contentType)
            };
        }

        [Fact]
        public async Task VerifyFaceAsync_SuccessfulResponse_IsMatchTrue_ReturnsTrue()
        {
            // Arrange
            var responseMessage = CreateResponse(HttpStatusCode.OK, @"{
                ""data"": {
                    ""isMatch"": true
                }
            }");

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString() == "https://api.fpt.ai/dmp/checkface/v1"),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _fptAIRepository.VerifyFaceAsync(_cccdImageMock.Object, _selfieImageMock.Object);

            // Assert
            Assert.True(result);
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task VerifyFaceAsync_SuccessfulResponse_IsMatchFalse_ReturnsFalse()
        {
            // Arrange
            var responseMessage = CreateResponse(HttpStatusCode.OK, @"{
                ""data"": {
                    ""isMatch"": false
                }
            }");

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString() == "https://api.fpt.ai/dmp/checkface/v1"),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _fptAIRepository.VerifyFaceAsync(_cccdImageMock.Object, _selfieImageMock.Object);

            // Assert
            Assert.False(result);
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}