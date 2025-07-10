using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class CandidateCv
    {
        public CandidateCv()
        {
            CvrequirementEvaluations = new HashSet<CvrequirementEvaluation>();
        }

        public int CandidateCvId { get; set; }
        public int? AccountId { get; set; }
        public int? InternshipId { get; set; }
        public string? Cvurl { get; set; }
        public DateTime? CreateAt { get; set; }
        public string? Status { get; set; }

        public virtual Account? Account { get; set; }
        public virtual InternshipPost? Internship { get; set; }
        public virtual ICollection<CvrequirementEvaluation> CvrequirementEvaluations { get; set; }
    }
}
