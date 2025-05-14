namespace API.DTO.AuthDTO
{
    public class ResLoginDTO
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string AccessToken { get; set; }
    }
}
