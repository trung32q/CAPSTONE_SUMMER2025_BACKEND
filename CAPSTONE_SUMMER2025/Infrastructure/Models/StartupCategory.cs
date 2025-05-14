using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class StartupCategory
    {
        public int StartupCategoryId { get; set; }
        public int? StartupId { get; set; }
        public int? CategoryId { get; set; }

        public virtual Category? Category { get; set; }
        public virtual Startup? Startup { get; set; }
    }
}
