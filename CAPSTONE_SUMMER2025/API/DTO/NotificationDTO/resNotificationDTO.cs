namespace API.DTO.NotificationDTO
{
    public class resNotificationDTO
    {
        public int NotificationId { get; set; }
        public int AccountId { get; set; }
        public string AvartarURL { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
