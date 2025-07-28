namespace API.DTO.Mesage
{
    public class EnsureRoomDto
    {
        public int AccountId { get; set; }
        public int? TargetAccountId { get; set; }
        public int? TargetStartupId { get; set; }
    }
}
