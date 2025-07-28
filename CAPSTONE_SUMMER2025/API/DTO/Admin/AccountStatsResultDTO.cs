namespace API.DTO.Admin
{
    public class DailyAccountStatDTO
    {
        public DateTime Date { get; set; }
        public int AccountCount { get; set; }
    }

    public class AccountStatsResultDTO
    {
        public int TotalNewAccountCount { get; set; }
        public List<DailyAccountStatDTO> DailyAccountStats { get; set; }
    }

}
