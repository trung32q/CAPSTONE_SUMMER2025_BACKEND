namespace API.DTO.Mesage
{
    public class UserMessageDto
    {
        public int ChatRoomId { get; set; }
        public int? SenderAccountId { get; set; }
        public int? SenderStartupId { get; set; }
        public string? Content { get; set; }
        public IFormFile? File { get; set; }
        public string? Type { get; set; }
    }
}
