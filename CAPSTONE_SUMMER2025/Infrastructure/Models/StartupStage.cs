using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class StartupStage
    {
        public StartupStage()
        {
            Startups = new HashSet<Startup>();
        }

        public int StageId { get; set; }
        public string StageName { get; set; } = null!;
        public string? Description { get; set; }

        public virtual ICollection<Startup> Startups { get; set; }
    }
}
