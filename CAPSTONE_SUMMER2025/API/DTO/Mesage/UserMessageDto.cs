namespace API.DTO.Mesage
{
    public class UserMessageDto
    {
        public int ChatRoomId { get; set; }
        public int? SenderAccountId { get; set; }
        public int? SenderStartupId { get; set; }
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public string? FileType { get; set; }
    }
}
