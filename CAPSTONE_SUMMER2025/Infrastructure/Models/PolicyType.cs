using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class PolicyType
    {
        public PolicyType()
        {
            Policies = new HashSet<Policy>();
        }

        public int PolicyTypeId { get; set; }
        public string? TypeName { get; set; }

        public virtual ICollection<Policy> Policies { get; set; }
    }
}
