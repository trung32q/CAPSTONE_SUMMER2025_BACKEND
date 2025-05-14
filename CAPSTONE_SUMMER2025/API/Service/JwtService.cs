using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text; // Thêm using này nếu bạn sử dụng Encoding.UTF8 cho key (thường không cần nếu key đã là base64)
using API.DTO.AuthDTO; 
using Microsoft.Extensions.Configuration;
using System; 
namespace API.Service
{
    public class JwtService
    {
        private readonly IConfiguration _configuration; 
        private readonly SymmetricSecurityKey _signingKey; 

        private const string SecurityAlgorithm = Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            // Khởi tạo signingKey một lần khi service được tạo
            var keyBytes = Convert.FromBase64String(_configuration["Jwt:Key"]);
            _signingKey = new SymmetricSecurityKey(keyBytes);
        }

        public string CreateAccessToken(ResLoginDTO resLogin)
        {
            var validity = DateTime.UtcNow.AddSeconds(_configuration.GetValue<int>("Jwt:AccessTokenExpiration")); 

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, resLogin.Id.ToString()), // Sử dụng tên claim chuẩn cho Subject (user id)
                new Claim(JwtRegisteredClaimNames.Email, resLogin.Email),        // Sử dụng tên claim chuẩn cho Email
                new Claim(ClaimTypes.Role, resLogin.Role),                       // Sử dụng ClaimTypes.Role cho vai trò
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Thêm JTI (JWT ID) để token là duy nhất
            };

            return GenerateToken(claims, validity);
        }

        public string CreateRefreshToken(ResLoginDTO resLogin)
        {
            // Refresh token thường có thời gian hết hạn dài hơn nhiều so với access token
            var validity = DateTime.UtcNow.AddSeconds(_configuration.GetValue<int>("Jwt:RefreshTokenExpiration")); // Sử dụng UtcNow và cấu hình riêng

            // Refresh token có thể chỉ cần chứa ID người dùng để tra cứu, hoặc thêm JTI để theo dõi
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, resLogin.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JTI cũng quan trọng cho refresh token để có thể thu hồi
                // Không nhất thiết phải có email, role trong refresh token nếu không cần thiết cho việc làm mới
            };

            return GenerateToken(claims, validity);
        }

        private string GenerateToken(Claim[] claims, DateTime expires)
        {
            var notBefore = DateTime.UtcNow; // Sử dụng UtcNow

            var jwt = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                notBefore: notBefore,
                expires: expires,
                signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithm)
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        // Không cần phương thức GetSecretKey() riêng nữa vì _signingKey đã được khởi tạo trong constructor

        /// <summary>
        /// Xác thực một token và trả về ClaimsPrincipal.
        /// Cho phép bỏ qua kiểm tra thời gian sống của token (hữu ích khi lấy claims từ access token đã hết hạn).
        /// </summary>
        /// <param name="token">Chuỗi JWT token.</param>
        /// <param name="validateLifetime">True để kiểm tra thời gian sống, False để bỏ qua.</param>
        /// <returns>ClaimsPrincipal nếu token hợp lệ, ngược lại là null.</returns>
        public ClaimsPrincipal GetPrincipalFromToken(string token, bool validateLifetime = true)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _signingKey,

                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],

                    ValidateLifetime = validateLifetime, // Quan trọng: cho phép bỏ qua kiểm tra hết hạn
                    ClockSkew = TimeSpan.Zero // Không cho phép chênh lệch thời gian
                };

                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
                return principal;
            }
            catch (SecurityTokenException) // Bắt các lỗi cụ thể liên quan đến token
            {
                // Token không hợp lệ (chữ ký sai, sai issuer/audience, hoặc hết hạn nếu validateLifetime = true)
                return null;
            }
            catch (Exception) // Bắt các lỗi khác nếu có
            {
                // Lỗi không mong muốn
                return null;
            }
        }
    }
}