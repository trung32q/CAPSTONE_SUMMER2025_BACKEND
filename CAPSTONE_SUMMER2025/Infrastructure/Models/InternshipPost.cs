using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class InternshipPost
    {
        public InternshipPost()
        {
            CandidateCvs = new HashSet<CandidateCv>();
            CvrequirementEvaluations = new HashSet<CvrequirementEvaluation>();
        }

        public int InternshipId { get; set; }
        public int? StartupId { get; set; }
        public int? PositionId { get; set; }
        public string? Description { get; set; }
        public string? Requirement { get; set; }
        public string? Benefits { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? Deadline { get; set; }
        public string? Status { get; set; }
        public string? Address { get; set; }
        public string? Salary { get; set; }

        public virtual PositionRequirement? Position { get; set; }
        public virtual Startup? Startup { get; set; }
        public virtual ICollection<CandidateCv> CandidateCvs { get; set; }
        public virtual ICollection<CvrequirementEvaluation> CvrequirementEvaluations { get; set; }
    }
}
