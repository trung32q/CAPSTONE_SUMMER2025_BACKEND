using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using API.DTO.PolicyDTO;
using API.Repositories.Interfaces;
using API.Service;
using Infrastructure.Models;

namespace API.Repositories
{
    public class ChatGPTRepository : IChatGPTRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ChatGPTRepository(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> CheckPostPolicyAsync(string postContent, List<resPolicyDTO> policies)
        {
            var systemPrompt = _configuration["OpenAI:SystemPrompt"];

            var policiesText = "";
            foreach (var policy in policies) 
            {
                policiesText += policy.Description;
            }

            var userPrompt = $@"
                 Chính sách:
                     {policiesText}

                  Bài viết:
                     {postContent}

                  Trả lời:
                           ";

            var request = new
            {
                model = "gpt-4",
                messages = new[]
                {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
                temperature = 0.2
            };

            var reqContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _configuration["OpenAI:ApiKey"]);

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", reqContent);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(result);
            return json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }


        public async Task<string> CheckPostPolicyWithUploadedImageAsync(string postContent, IFormFile imageFile, List<resPolicyDTO> policies)
        {
            var systemPrompt = _configuration["OpenAI:SystemPrompt"];

            var policiesText = "";
            foreach (var policy in policies)
            {
                policiesText += policy.Description;
            }

            // Bước 1: Chuyển ảnh sang base64
            string base64Image;
            using (var ms = new MemoryStream())
            {
                await imageFile.CopyToAsync(ms);
                var imageBytes = ms.ToArray();
                base64Image = Convert.ToBase64String(imageBytes);
            }

            // Bước 2: Tạo prompt
            var userPrompt = $@"
Chính sách:
{policiesText}

Bài viết:
{postContent}

Hình ảnh kèm theo:
(ảnh ở bên dưới)
";

            // Bước 3: Gửi đến OpenAI
            var request = new
            {
                model = "gpt-4o",
                messages = new object[]
                {
            new { role = "system", content = systemPrompt },
            new
            {
                role = "user",
                content = new object[]
                {
                    new { type = "text", text = userPrompt },
                    new {
                        type = "image_url",
                        image_url = new {
                            url = $"data:{imageFile.ContentType};base64,{base64Image}"
                        }
                    }
                }
            }
                },
                max_tokens = 1000
            };

            var reqContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _configuration["OpenAI:ApiKey"]);

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", reqContent);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"OpenAI API returned {(int)response.StatusCode} - {response.ReasonPhrase}: {errorBody}");
            }
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(result);
            return json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }



        public async Task<string> CheckPostPolicyWithVideoAsync(string postContent, IFormFile videoFile, List<resPolicyDTO> policies)
        {
            var systemPrompt = _configuration["OpenAI:SystemPrompt"];

            var policiesText = "";
            foreach (var policy in policies)
            {
                policiesText += policy.Description;
            }

            // Bước 1: Lưu video tạm vào file hệ thống
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(videoFile.FileName));
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await videoFile.CopyToAsync(stream);
            }

            // Bước 2: Trích xuất frame từ video
            var framePaths = VideoFrameExtractor.ExtractFrames(tempPath, frameRate: 0.2); // mỗi 5 giây

            // Bước 3: Duyệt từng frame
            foreach (var framePath in framePaths)
            {
                var imageBytes = await File.ReadAllBytesAsync(framePath);
                var base64Image = Convert.ToBase64String(imageBytes);

                var userPrompt = $@"
Chính sách:
{policiesText}

Bài viết:
{postContent}

Ảnh từ video:
(xem bên dưới)
";

                var request = new
                {
                    model = "gpt-4o",
                    messages = new object[]
                    {
                new { role = "system", content = systemPrompt },
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = userPrompt },
                        new {
                            type = "image_url",
                            image_url = new {
                                url = $"data:image/jpeg;base64,{base64Image}"
                            }
                        }
                    }
                }
                    },
                    max_tokens = 1000
                };

                var reqContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _configuration["OpenAI:ApiKey"]);

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", reqContent);
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"OpenAI API returned {(int)response.StatusCode} - {response.ReasonPhrase}: {errorBody}");
                }

                var result = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(result);
                var reply = json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

                if (reply != null && reply.ToLower().Contains("vi phạm"))
                {
                    return $"Vi phạm: {reply}";
                }
            }

            // Dọn dẹp file video tạm
            try { File.Delete(tempPath); } catch { }

            return "Hợp lệ";
        }


    }
}
