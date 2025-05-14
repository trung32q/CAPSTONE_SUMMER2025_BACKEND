using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class BusinessModelCanva
    {
        public int BmcId { get; set; }
        public int? StartupId { get; set; }
        public string? CustomerSegments { get; set; }
        public string? ValuePropositions { get; set; }
        public string? Channels { get; set; }
        public string? CustomerRelationships { get; set; }
        public string? RevenueStreams { get; set; }
        public string? KeyResources { get; set; }
        public string? KeyActivities { get; set; }
        public string? KeyPartners { get; set; }
        public string? Cost { get; set; }

        public virtual Startup? Startup { get; set; }
    }
}
