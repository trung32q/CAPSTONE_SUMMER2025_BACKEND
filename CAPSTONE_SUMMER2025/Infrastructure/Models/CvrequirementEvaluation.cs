using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class CvrequirementEvaluation
    {
        public int EvaluationId { get; set; }
        public int CandidateCvId { get; set; }
        public int InternshipId { get; set; }
        public string? EvaluationTechSkills { get; set; }
        public string? EvaluationExperience { get; set; }
        public string? EvaluationSoftSkills { get; set; }
        public string? EvaluationOverallSummary { get; set; }

        public virtual CandidateCv CandidateCv { get; set; } = null!;
        public virtual InternshipPost Internship { get; set; } = null!;
    }
}
