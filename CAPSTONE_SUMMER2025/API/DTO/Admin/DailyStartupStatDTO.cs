namespace API.DTO.Admin
{
    public class DailyStartupStatDTO
    {
        public DateTime Date { get; set; }
        public int StartupCount { get; set; }
    }

    public class StartupStatsResultDTO
    {
        public int TotalNewStartupCount { get; set; }
        public List<DailyStartupStatDTO> DailyStartupStats { get; set; }
    }

}
