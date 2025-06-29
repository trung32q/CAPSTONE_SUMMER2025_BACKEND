namespace API.DTO.DartBoardDTO
{
    public class DartBoardResultDTO
    {
        public ClickStatsResultDTO clickStatsResultDTO { get; set; } = new();
        public SubscribeStatsResultDTO subscribeStatsResultDTO { get; set; } = new();
        public InteractionStatsResultDTO interactionStatsResultDTO { get; set; } = new();
        public InternshipPostStatsResultDTO internshipPostStatsResultDTO { get; set; } = new();
        public PostStatsResultDTO postStatsResultDTO { get; set; } = new();
    }
}
