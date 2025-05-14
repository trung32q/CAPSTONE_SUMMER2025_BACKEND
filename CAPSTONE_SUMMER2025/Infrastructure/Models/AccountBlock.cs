using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class AccountBlock
    {
        public int BlockId { get; set; }
        public int? BlockerAccountId { get; set; }
        public int? BlockedAccountId { get; set; }
        public DateTime? BlockedAt { get; set; }

        public virtual Account? BlockedAccount { get; set; }
        public virtual Account? BlockerAccount { get; set; }
    }
}
