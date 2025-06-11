namespace API.DTO.StartupDTO
{
    public class CreateChatRoomDTO
    {
        public string RoomName { get; set; }
        public int StartupId { get; set; }
        public int CreatorAccountId { get; set; }
        public string? MemberTitle {  get; set; }
    }
}
