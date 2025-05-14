using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class StartupImage
    {
        public int StartupImageId { get; set; }
        public int? StartupId { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageTitle { get; set; }

        public virtual Startup? Startup { get; set; }
    }
}
