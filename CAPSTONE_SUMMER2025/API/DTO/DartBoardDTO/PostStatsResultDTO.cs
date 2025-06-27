namespace API.DTO.DartBoardDTO
{
    public class PostStatsResultDTO
    {
        public int TotalPostCount { get; set; }
        public List<DailyPostStatDTO> DailyPostStats { get; set; } = new();
    }
}
