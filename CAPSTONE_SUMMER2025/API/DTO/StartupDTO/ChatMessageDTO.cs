namespace API.DTO.StartupDTO
{
    public class ChatMessageDTO
    {
        public int AccountId { get; set; }
        public string MemberTitle { get; set; }
        public string MessageContent { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string AvatarUrl { get; set; }
        public int MessageId {  get; set; }
        public int ChatRoomId {  get; set; }
        public string MessageType {  get; set; }
    }
}
