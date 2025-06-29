namespace API.DTO.DartBoardDTO
{
    public class InteractionStatsResultDTO
    {
        public int TotalInteractionCount { get; set; }
        public List<DailyInteractionStatDTO> DailyInteractionStats { get; set; } = new();
    }
}
