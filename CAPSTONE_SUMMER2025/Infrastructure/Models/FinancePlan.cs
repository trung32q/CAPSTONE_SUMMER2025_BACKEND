using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class FinancePlan
    {
        public int FinancePlanId { get; set; }
        public int? StartupId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public string? FinanceplanSheetUrl { get; set; }

        public virtual Startup? Startup { get; set; }
    }
}
