namespace API.DTO.StartupDTO
{
    public class StartupMemberDTO
    {
        public int AccountId { get; set; }
        public int Memberid { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; }
        public string AvatarUrl {  get; set; }
        public DateTime JoinAT { get; set; }
        public string Email {  get; set; }

    }
}
