namespace API.DTO.Mesage
{
    public class GetUserMessageDTO
    {
        public int MessageId { get; set; }
        public int? ChatRoomId { get; set; }
        public int? SenderAccountId { get; set; }
        public int? SenderStartupId { get; set; }
        public string? Content { get; set; }
        public string? Type { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }

        public string? Name {  get; set; }
        public string? AvatarUrl {  get; set; }
    }
}
