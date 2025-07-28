namespace API.DTO.Mesage
{
    public class ResUserMessageDTO
    {

        public int ChatRoomId { get; set; }
        public string SenderName { get; set; }
        public string SenderAvatar { get; set; }
        public string? Content { get; set; }
        public IFormFile? File { get; set; }
        public string? Type { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }
}
