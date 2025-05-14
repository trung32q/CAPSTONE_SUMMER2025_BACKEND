using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class Policy
    {
        public int PolicyId { get; set; }
        public string? Description { get; set; }
        public int? PolicyTypeId { get; set; }
        public DateTime? CreateAt { get; set; }
        public bool? IsActive { get; set; }

        public virtual PolicyType? PolicyType { get; set; }
    }
}
