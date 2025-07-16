using API.DTO.StartupDTO;

namespace API.DTO.PostDTO
{
    public class CandidateCVResponseDTO
    {
        public int CandidateCV_ID { get; set; }
        public string CVURL { get; set; }
        public DateTime CreateAt { get; set; }
        public string Status { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PositionRequirement { get; set; }
        public int PositionId {  get; set; }

        public string AvatarUrl {  get; set; }
        public int AccountId {  get; set; }

        public CVRequirementEvaluationResultDto CVRequirementEvaluation {  get; set; }
    }
}
