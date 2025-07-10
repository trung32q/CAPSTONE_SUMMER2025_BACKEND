namespace API.DTO.StartupDTO
{
    public class CVRequirementEvaluationResultDto
    {
        public string Evaluation_TechSkills { get; set; }
        public string Evaluation_Experience { get; set; }
        public string Evaluation_SoftSkills { get; set; }
        public string Evaluation_OverallSummary { get; set; }

        public int? CandidateCVID {  get; set; }
        public int? InternshipId { get; set; }
    }
}
