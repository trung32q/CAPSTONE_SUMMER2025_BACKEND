using API.DTO.AccountDTO;
using API.Repositories;
using API.Repositories.Interfaces;
using API.Service.Interface;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
namespace API.Service
{
    public class CCCDService : ICCCDService
    {
        private readonly IFptAIRepository _fptAIRepository;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CCCDService(HttpClient httpClient, IConfiguration configuration, IFptAIRepository fptAIRepository)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _fptAIRepository = fptAIRepository;
        }

        public async Task<ExtractedCccdInfoDTO> ExtractCccdInfoAsync(IFormFile front, IFormFile back)
        {
            var base64Front = await ConvertToBase64(front);
            var base64Back = await ConvertToBase64(back);

            var systemPrompt = _configuration["OpenAI:CccdPrompt"] ?? "Bạn là một AI trích xuất dữ liệu từ CCCD Việt Nam.";

            var userPrompt = "Hãy trích xuất và trả về JSON với các trường: FullName, CccdNumber, DateOfBirth (yyyy-MM-dd), Gender, Address, IssueDate (yyyy-MM-dd), IssuePlace. Nếu ảnh mờ, không thể đọc rõ, hãy trả về null.";

            var requestBody = new
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
                        new { type = "image_url", image_url = new { url = $"data:{front.ContentType};base64,{base64Front}" } },
                        new { type = "image_url", image_url = new { url = $"data:{back.ContentType};base64,{base64Back}" } }
                    }
                }
                },
                max_tokens = 700
            };

            var reqContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _configuration["OpenAI:ApiKey"]);

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", reqContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                throw new Exception($"GPT OCR thất bại: {errorText}");
            }

            var result = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(result);
            var content = json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            if (string.IsNullOrWhiteSpace(content) || content.ToLower().Contains("null"))
                return null;

            try
            {
                return JsonSerializer.Deserialize<ExtractedCccdInfoDTO>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return null;
            }
        }

        private async Task<string> ConvertToBase64(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return Convert.ToBase64String(ms.ToArray());
        }

        public async Task<CccdVerificationResultDTO> VerifyCccdAsync(IFormFile cccdImage, IFormFile selfieImage)
        {
            var faceMatch = await _fptAIRepository.VerifyFaceAsync(cccdImage, selfieImage);

            return new CccdVerificationResultDTO
            {
                IsMatch = faceMatch,
                ExtractedInfo = faceMatch ? "Khuôn mặt trùng khớp" : "Khuôn mặt không trùng khớp"
            };
        }

        public List<string> CompareWithUserInput(ExtractedCccdInfoDTO extracted, CccdVerificationRequestDTO input)
        {
            var mismatches = new List<string>();

            if (!string.Equals(extracted.FullName, input.FullName, StringComparison.OrdinalIgnoreCase))
                mismatches.Add("FullName");

            if (!string.Equals(extracted.CccdNumber, input.CccdNumber, StringComparison.OrdinalIgnoreCase))
                mismatches.Add("CccdNumber");

            if (!string.Equals(extracted.DateOfBirth, input.DateOfBirth))
                mismatches.Add("DateOfBirth");

            if (!string.Equals(extracted.Gender, input.Gender, StringComparison.OrdinalIgnoreCase))
                mismatches.Add("Gender");

            if (!string.Equals(extracted.Address, input.Address, StringComparison.OrdinalIgnoreCase))
                mismatches.Add("Address");

            if (!string.Equals(extracted.IssueDate, input.IssueDate))
                mismatches.Add("IssueDate");

            if (!string.Equals(extracted.IssuePlace, input.IssuePlace, StringComparison.OrdinalIgnoreCase))
                mismatches.Add("IssuePlace");

            return mismatches;
        }
    }
}
