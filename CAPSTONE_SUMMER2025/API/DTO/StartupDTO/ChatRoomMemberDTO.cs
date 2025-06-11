namespace API.DTO.StartupDTO
{
    public class ChatRoomMemberDTO
    {
        public int AccountId { get; set; }
        public string FullName { get; set; }
        public string MemberTitle { get; set; }
        public bool CanAdministerChannel { get; set; }
        public string AvatarUrl {  get; set; }
    }
}
