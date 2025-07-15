using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class StartupPitching
    {
        public int PitchingId { get; set; }
        public int StartupId { get; set; }
        public string Type { get; set; } = null!;
        public string Link { get; set; } = null!;
        public DateTime? CreateAt { get; set; }

        public virtual Startup Startup { get; set; } = null!;
    }
}
