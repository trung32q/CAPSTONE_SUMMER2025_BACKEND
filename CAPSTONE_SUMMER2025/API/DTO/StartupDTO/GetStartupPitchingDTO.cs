namespace API.DTO.StartupDTO
{
    public class GetStartupPitchingDTO
    {
        public int PitchingId { get; set; }
        public string? Type { get; set; }
        public string Link { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
