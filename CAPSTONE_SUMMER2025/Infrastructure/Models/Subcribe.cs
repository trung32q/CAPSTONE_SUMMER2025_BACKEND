using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class Subcribe
    {
        public int SubcribeId { get; set; }
        public int? FollowerAccountId { get; set; }
        public int? FollowingStartUpId { get; set; }
        public DateTime? FollowDate { get; set; }

        public virtual Account? FollowerAccount { get; set; }
        public virtual Startup? FollowingStartUp { get; set; }
    }
}
