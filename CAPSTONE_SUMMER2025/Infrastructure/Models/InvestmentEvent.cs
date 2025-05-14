using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class InvestmentEvent
    {
        public InvestmentEvent()
        {
            InvestmentEventTickets = new HashSet<InvestmentEventTicket>();
            InvestmentEventsRequests = new HashSet<InvestmentEventsRequest>();
        }

        public int EventId { get; set; }
        public int? StartupId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? Schedule { get; set; }
        public string? Location { get; set; }

        public virtual Startup? Startup { get; set; }
        public virtual ICollection<InvestmentEventTicket> InvestmentEventTickets { get; set; }
        public virtual ICollection<InvestmentEventsRequest> InvestmentEventsRequests { get; set; }
    }
}
