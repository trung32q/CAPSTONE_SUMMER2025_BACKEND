namespace API.DTO.DartBoardDTO
{
    public class InternshipPostStatsResultDTO
    {
        public int TotalInternshipPostCount { get; set; }
        public List<DailyInternshipPostStatDTO> DailyInternshipPostStats { get; set; } = new();
    }
}
