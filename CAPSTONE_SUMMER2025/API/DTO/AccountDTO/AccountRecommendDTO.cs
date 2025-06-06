namespace API.DTO.AccountDTO
{
    public class AccountRecommendDTO
    {
        public int AccountId { get; set; }
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
        public int TotalFollowers { get; set; }
        public string Position { get; set; }

    }
}
