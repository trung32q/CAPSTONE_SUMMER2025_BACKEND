namespace API.DTO.StartupDTO
{
    public class PositionRequirementResponseDto
    {
        public int? PositionId { get; set; }
        public int? StartupId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Requirement { get; set; }
    }
}
