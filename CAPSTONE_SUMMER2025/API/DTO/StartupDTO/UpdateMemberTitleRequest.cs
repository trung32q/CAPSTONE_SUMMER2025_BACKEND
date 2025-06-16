namespace API.DTO.StartupDTO
{
    public class UpdateMemberTitleRequest
    {
        public int ChatRoom_ID { get; set; }
        public int Account_ID { get; set; }
        public string MemberTitle { get; set; }
    }
}
