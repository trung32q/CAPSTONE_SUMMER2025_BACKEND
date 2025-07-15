namespace API.DTO.StartupDTO
{
    public class SwotAnalysisDto
    {
        public List<string> Strengths { get; set; }
        public List<string> Weaknesses { get; set; }
        public List<string> Opportunities { get; set; }
        public List<string> Threats { get; set; }
        public string Recommendation { get; set; }
    }
}
