namespace API.DTO.DartBoardDTO
{
    public class SubscribeStatsResultDTO
    {
        public int TotalSubcribeCount { get; set; }
        public List<DailySubscribeStatDTO> DailySubcribeStats { get; set; } = new();
    }
}
