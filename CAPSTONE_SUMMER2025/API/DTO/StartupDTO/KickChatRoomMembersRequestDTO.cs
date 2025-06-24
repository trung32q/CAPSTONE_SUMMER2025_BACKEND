namespace API.DTO.StartupDTO
{
    public class KickChatRoomMembersRequestDTO
    {
        public int ChatRoomId { get; set; }
        public int RequesterAccountId { get; set; }  // người yêu cầu kick
        public List<int> TargetAccountIds { get; set; } = new();
    }
}
