namespace API.DTO.StartupDTO
{
    public class CreateInviteDTO
    {
        public int Account_ID { get; set; }    // Người được mời
        public int Startup_ID { get; set; }
        public int Role_ID { get; set; }
        public int InviteBy { get; set; }      // Người gửi
    }
}
