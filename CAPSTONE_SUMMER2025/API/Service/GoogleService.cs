using API.DTO.AuthDTO;
using Google.Apis.Auth;

namespace API.Service
{
    public class GoogleService
    {

        private IConfiguration _configuration;
        public GoogleService(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async Task<ResGoogleInfoDTO> ValidateGoogleTokenAsync(string token)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new List<string> { _configuration["Authentication:Google:ClientId"] }
            };

            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);
                return new ResGoogleInfoDTO
                {
                    Email = payload.Email,
                    Name = payload.Name,
                    Picture = payload.Picture
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
