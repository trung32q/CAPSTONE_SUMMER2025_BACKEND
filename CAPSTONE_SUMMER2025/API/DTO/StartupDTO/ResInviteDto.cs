namespace API.DTO.StartupDTO
{
    public class ResInviteDto
    {
        public int InviteId { get; set; }
        public string? SenderAvatar{ get; set; }
        public string? SenderEmail { get; set; }
        public string? Receiveravatar { get; set; }
        public string? ReceiverEmail { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public int? StartupId { get; set; }
        public DateTime? InviteSentAt { get; set; }
        public string? InviteStatus { get; set; }
    }
}
