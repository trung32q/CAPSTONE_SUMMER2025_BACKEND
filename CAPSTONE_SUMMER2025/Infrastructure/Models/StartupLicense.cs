using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class StartupLicense
    {
        public int LicenseId { get; set; }
        public int? StartupId { get; set; }
        public string? LicenseName { get; set; }
        public string? LicenseNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? IssuedBy { get; set; }
        public string? Description { get; set; }

        public virtual Startup? Startup { get; set; }
    }
}
