using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class Follow
    {
        public int FollowId { get; set; }
        public int? FollowerAccountId { get; set; }
        public int? FollowingAccountId { get; set; }
        public DateTime? FollowDate { get; set; }

        public virtual Account? FollowerAccount { get; set; }
        public virtual Account? FollowingAccount { get; set; }
    }
}
