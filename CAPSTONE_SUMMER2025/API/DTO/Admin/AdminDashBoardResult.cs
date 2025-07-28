namespace API.DTO.Admin
{
    public class AdminDashBoardResult
    {
        public int NumAccountActive {  get; set; }
        public int NumAccount {  get; set; }
        public AccountStatsResultDTO AccountsCreatedLast7Days {  get; set; }
        public int NumStartup {  get; set; }
        public StartupStatsResultDTO StartupsCreatedLast7Days { get; set; }
        public GrowthStatsDtocs GrowthAccountStatsDtocs { get; set; }

    }
}
