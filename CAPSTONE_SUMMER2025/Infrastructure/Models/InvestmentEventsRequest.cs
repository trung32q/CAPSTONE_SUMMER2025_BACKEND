using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class InvestmentEventsRequest
    {
        public int RequestId { get; set; }
        public int? EventId { get; set; }
        public int? AccountId { get; set; }
        public string? Status { get; set; }

        public virtual Account? Account { get; set; }
        public virtual InvestmentEvent? Event { get; set; }
    }
}
