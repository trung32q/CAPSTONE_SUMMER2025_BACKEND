namespace API.DTO.StartupDTO
{
    public class SendMessageRequest
    {
        public int ChatRoomId { get; set; }
        public int AccountId { get; set; }
        public string MessageContent { get; set; }
    }
}
