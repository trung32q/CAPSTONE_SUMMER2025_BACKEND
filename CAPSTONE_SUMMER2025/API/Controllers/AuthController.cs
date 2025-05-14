using API.Attributes;
using API.DTO.AccountDTO;
using API.DTO.AuthDTO;
using API.Repositories.Interfaces;
using API.Service;
using AutoMapper;
using Infrastructure.Models;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;
        private readonly JwtService _jwtService;
        private readonly GoogleService _googleService;
        private IConfiguration _configuration;

        public AuthController(IAuthRepository authRepository, IMapper mapper , JwtService jwtService, IConfiguration configuration, GoogleService googleService, IAccountRepository accountRepository)
        {
            _authRepository = authRepository;
            _mapper = mapper;
            _jwtService = jwtService;
            _configuration = configuration;
            _googleService = googleService;
            _accountRepository = accountRepository;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] ReqAccountDTO req)
        {
            try
            {
                var account = await _authRepository.Register(req);
                var resAccount = _mapper.Map<ResAccountDTO>(account);

                return Ok(resAccount);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("Login")]
        [ApiMessage("Sign in by account")]
        public async Task<ActionResult<ResLoginDTO>> Signin([FromBody] LoginDTO loginDTO)
        {
            ResLoginDTO res = new ResLoginDTO();

            var accountCurrentDB = await _authRepository.Login(loginDTO);

            if (accountCurrentDB == null)
            {
                return Unauthorized();
            }

            res = _mapper.Map<ResLoginDTO>(accountCurrentDB);
            string access_token = _jwtService.CreateAccessToken(res); 
                                                                     
            string refresh_token = _jwtService.CreateRefreshToken(res);

            // Lưu refresh token vào cơ sở dữ liệu (hoặc hash của nó)
            // Đảm bảo bạn có cơ chế để xử lý việc ghi đè hoặc thêm mới token một cách an toàn
            await _authRepository.UpdateAccountRefreshTokenAsync(accountCurrentDB.AccountId, refresh_token, DateTime.UtcNow.AddSeconds(_configuration.GetValue<int>("Jwt:RefreshTokenExpiration")));

            var resCookie = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Đặt thành true trong môi trường production
                SameSite = SameSiteMode.Strict, 
                Expires = DateTime.UtcNow.AddSeconds(_configuration.GetValue<int>("Jwt:RefreshTokenExpiration"))
            };

            Response.Cookies.Append("refresh_token", refresh_token, resCookie);

         
            return Ok(new { AccessToken = access_token });
        }
        [HttpPost("Login-google")]
        [ApiMessage("Sign in with google")]
        public async Task<ActionResult<ResLoginDTO>> SigninByGoogle([FromBody] ReqGoogleLoginDTO googleLoginDTO)
        {
            var googleInfo = await _googleService.ValidateGoogleTokenAsync(googleLoginDTO.GoogleToken);

            if (googleInfo == null)
            {
                return NotFound();
            }

            var accountCurrentDB = await _accountRepository.GetAccountByEmailAsync(googleInfo.Email);
            if (accountCurrentDB == null)
            {
                return Unauthorized();
            }

            var res = _mapper.Map<ResLoginDTO>(accountCurrentDB);

            string access_token = _jwtService.CreateAccessToken(res);
            res.AccessToken = access_token;

            string refresh_token = _jwtService.CreateRefreshToken(res);
            //await _accountRepository.UpdateAccountRefreshTokenAsync(refresh_token, res.Email);

            var resCookie = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddSeconds(_configuration.GetValue<int>("Jwt:RefreshTokenExpiration"))
            };

            Response.Cookies.Append("refresh_token", refresh_token, resCookie);

            return Ok(res.AccessToken);
        }

        [HttpPost("VerifyOtp")]
        public async Task<IActionResult> VerifyOtp([FromBody] ReqOtpDTO req)
        {
            try
            {    
                var Account = await _accountRepository.GetAccountByEmailAsync(req.Email);
                 if (Account == null)
                    {
                        return NotFound(new { message = "Account not found" });
                    }
                var OTP = await _authRepository.GetActiveUserOtpAsync(Account.AccountId);
                bool isOtpValid = BCrypt.Net.BCrypt.Verify(req.Otp, OTP.OtpCode);

                if (!isOtpValid)
                {
                    return BadRequest(new ResVerifyOtpDTO { Success = false, Message = "Mã OTP không chính xác." });
                }
                await _authRepository.UpdateStatusAccountAsync(req);
                return Ok(new ResVerifyOtpDTO
                {
                    Success = true,
                    Message = "Xác thực OTP thành công. Tài khoản của bạn đã được kích hoạt."                 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }      
        }
        [HttpPost("RefreshToken")]
        [AllowAnonymous] 
        public async Task<IActionResult> RefreshToken()
        {
            var oldRefreshTokenFromCookie = Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(oldRefreshTokenFromCookie))
            {
                return Unauthorized(new { message = "Refresh token is missing from cookie." });
            }

            // 1. Xác thực oldRefreshToken (chữ ký VÀ THỜI GIAN SỐNG của chính nó)         
            var principal = _jwtService.GetPrincipalFromToken(oldRefreshTokenFromCookie, validateLifetime: true);

            if (principal == null)
            {
                // Nếu refresh token không hợp lệ 
                // Xóa cookie cũ để tránh client gửi lại token không hợp lệ.
                Response.Cookies.Delete("refresh_token", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, 
                    SameSite = SameSiteMode.Strict
                });
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }

            // Lấy UserId từ claims của refresh token. Sử dụng JwtRegisteredClaimNames.Sub là chuẩn.
            var userId = principal.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                Response.Cookies.Delete("refresh_token");
                return Unauthorized(new { message = "Invalid refresh token claims: User ID missing." });
            }

            // 2. Kiểm tra oldRefreshToken trong cơ sở dữ liệu
            // Giả sử _authRepository.GetUserByIdAsync trả về đối tượng người dùng bao gồm RefreshToken và RefreshTokenExpiryTime
            var user = await _authRepository.GetUserById(int.Parse(userId));

            // Kiểm tra xem người dùng có tồn tại không, refresh token trong DB có khớp với token từ cookie không,
            // và thời gian hết hạn trong DB có còn hợp lệ không.
            if (user == null || user.RefreshToken != oldRefreshTokenFromCookie || user.RefreshTokenExpiry <= DateTime.UtcNow)
            {              
                Response.Cookies.Delete("refresh_token");          
                return Unauthorized(new { message = "Refresh token is invalid, revoked, or has been superseded." });
            }          
            var resLoginDtoForToken = _mapper.Map<ResLoginDTO>(user);

            string newAccessToken = _jwtService.CreateAccessToken(resLoginDtoForToken);

            string newRefreshToken = _jwtService.CreateRefreshToken(resLoginDtoForToken);
            var newRefreshTokenExpiryTime = DateTime.UtcNow.AddSeconds(_configuration.GetValue<int>("Jwt:RefreshTokenExpiration"));

            // Cập nhật refresh token mới và thời gian hết hạn trong DB
            await _authRepository.UpdateAccountRefreshTokenAsync(int.Parse(userId), newRefreshToken, newRefreshTokenExpiryTime);

            // Gửi refresh token MỚI qua cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict, 
                Expires = newRefreshTokenExpiryTime
            };
            Response.Cookies.Append("refresh_token", newRefreshToken, cookieOptions);
            return Ok(new { AccessToken = newAccessToken });
        }

    }
}

