using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class StartupClick
    {
        public int? StartupId { get; set; }
        public DateTime? DateClick { get; set; }
        public int StartupClickId { get; set; }

        public virtual Startup? Startup { get; set; }
    }
}
