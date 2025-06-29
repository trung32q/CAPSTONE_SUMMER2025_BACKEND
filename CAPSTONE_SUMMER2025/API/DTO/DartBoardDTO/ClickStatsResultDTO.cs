namespace API.DTO.DartBoardDTO
{
    public class ClickStatsResultDTO
    {
        public int TotalClickCount { get; set; }
        public List<DailyClickStatDTO> DailyClickStats { get; set; } = new();
    }
}
