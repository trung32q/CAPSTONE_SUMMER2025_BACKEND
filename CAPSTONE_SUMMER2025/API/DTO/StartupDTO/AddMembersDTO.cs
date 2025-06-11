namespace API.DTO.StartupDTO
{
    public class AddMembersDTO
    {
        public int ChatRoomId { get; set; }
        public int CurrentUserId { get; set; }
        public List<MemberToAddDTO> MembersToAdd { get; set; } = new();
    }
}
