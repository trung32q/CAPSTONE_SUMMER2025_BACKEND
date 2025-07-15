using API.DTO.StartupDTO;
using API.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace API.Service
{
    public class SwotService : ISwotService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
   

        public SwotService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;         
        }

        public async Task<SwotAnalysisDto> AnalyzeSwotAsync(SwotBmcDto bmcDto)
        {

            var prompt = $@"
I will provide you the Business Model Canvas of a startup and basic information about the startup as follows:

Startup Description: {bmcDto.StartupDescription}
- Customer Segments: {bmcDto.CustomerSegments}
- Value Propositions: {bmcDto.ValuePropositions}
- Channels: {bmcDto.Channels}
- Customer Relationships: {bmcDto.CustomerRelationships}
- Revenue Streams: {bmcDto.RevenueStreams}
- Key Resources: {bmcDto.KeyResources}
- Key Activities: {bmcDto.KeyActivities}
- Key Partners: {bmcDto.KeyPartners}
- Cost: {bmcDto.Cost}

Based on this information, **analyze the SWOT (Strengths, Weaknesses, Opportunities, Threats) for this startup in depth**:

- For each Strength, Weakness, Opportunity, and Threat, write at least 2-3 items.
- Each item should be 2-3 sentences with concrete details, examples, or explanations (not just bullet points).
- If possible, relate directly to the information above (for example, reference a specific Value Proposition or Partner).
- In Recommendation, provide a step-by-step action plan (at least 3 steps) for the startup's next development stage.

**Return your answer in pure JSON, matching exactly this schema:**

{{
  ""Strengths"": [ ""Full detailed strength 1."", ""Full detailed strength 2."" ],
  ""Weaknesses"": [ ""Full detailed weakness 1."", ""Full detailed weakness 2."" ],
  ""Opportunities"": [ ""Full detailed opportunity 1."", ""Full detailed opportunity 2."" ],
  ""Threats"": [ ""Full detailed threat 1."", ""Full detailed threat 2."" ],
  ""Recommendation"": ""Step-by-step plan, at least 3 steps.""
}}
";

            var apiKey = _config["OpenAI:ApiKey"];
            var systemPrompt = "You are a startup business analyst. Always reply in English and ONLY in JSON as the requested schema.";

            var requestData = new
            {
                model = "gpt-4o", // hoặc gpt-4, gpt-3.5-turbo tuỳ quota
                messages = new[]
                {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = prompt }
            },
                temperature = 0.4
            };

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await httpClient.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();

            // Parse kết quả
            using var doc = JsonDocument.Parse(responseString);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            // Nếu OpenAI trả về có dấu ```json ... ```
            content = content.Trim('`', 'j', 's', 'o', 'n', ' ');

            // Parse lại thành object
            var swot = JsonSerializer.Deserialize<SwotAnalysisDto>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return swot;
        }
    }
}
