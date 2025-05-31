namespace API.DTO.AccountDTO
{
    public class AccountSearchResultDTO
    {
        public int AccountId { get; set; }
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
        public string Position { get; set; }
        public string Workplace { get; set; }
        public int FollowerCount { get; set; } // 🔥 mới thêm

    }

}
