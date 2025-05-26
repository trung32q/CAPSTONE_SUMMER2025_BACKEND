using API.Repositories.Interfaces;
using System.Text.Json;

namespace API.Repositories
{
    public class FptAIRepository : IFptAIRepository
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public FptAIRepository(IHttpClientFactory factory, IConfiguration configuration)
        {
            _httpClientFactory = factory;
            _configuration = configuration;
        }



        public async Task<bool> VerifyFaceAsync(IFormFile cccdImage, IFormFile selfieImage)
        {
            var apiKey = _configuration["FPTAI:ApiKey"];
            var client = _httpClientFactory.CreateClient();

            var form = new MultipartFormDataContent();
            form.Add(new StreamContent(cccdImage.OpenReadStream()), "file[]", cccdImage.FileName);
            form.Add(new StreamContent(selfieImage.OpenReadStream()), "file[]", selfieImage.FileName);

            client.DefaultRequestHeaders.Add("api_key", apiKey);

            var response = await client.PostAsync("https://api.fpt.ai/dmp/checkface/v1", form);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if (root.TryGetProperty("data", out var dataElem) &&
                dataElem.TryGetProperty("isMatch", out var isMatchElem) &&
                isMatchElem.GetBoolean())
            {
                return true;
            }

            return false;
        }



    }
}
