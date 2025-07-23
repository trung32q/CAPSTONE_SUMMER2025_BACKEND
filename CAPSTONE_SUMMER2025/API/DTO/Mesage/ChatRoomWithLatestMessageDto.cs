namespace API.DTO.Mesage
{
    public class ChatRoomWithLatestMessageDto
    {
        public int ChatRoomId { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public string LatestMessageContent { get; set; }
        public DateTime? LatestMessageTime { get; set; }
        public string TargetName { get; set; }     // người còn lại trong chat
        public string TargetAvatar { get; set; }   // avatar người đó
    }
}
