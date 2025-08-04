namespace API.DTO.VideoCall
{
    public class ResStartCallDto
    {
        public Guid CallSessionId { get; set; }
        public string RoomToken { get; set; }
        public DateTime StartedAt { get; set; }
        public int ChatRoomId { get; set; }
        public string Status { get; set; }
        public UserShortDto Caller { get; set; }
        public UserShortDto Callee { get; set; }
    }
    public class UserShortDto
    {
        public int AccountId { get; set; }
        public string FullName { get; set; }
        public string? AvatarUrl { get; set; }
    }

}
