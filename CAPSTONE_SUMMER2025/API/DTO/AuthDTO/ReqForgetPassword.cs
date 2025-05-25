namespace API.DTO.AuthDTO
{
    public class ReqForgetPassword
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
