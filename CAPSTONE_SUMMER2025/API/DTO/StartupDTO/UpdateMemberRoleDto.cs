namespace API.DTO.StartupDTO
{
    public class UpdateMemberRoleDto
    {
        public int StartupId { get; set; }
        public int AccountId { get; set; }
        public int NewRoleId { get; set; }
    }
}
