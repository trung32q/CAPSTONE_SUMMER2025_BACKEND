using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class UserOtp
    {
        public int UserOtpId { get; set; }
        public string? OtpCode { get; set; }
        public string? Password { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool? IsUsed { get; set; }
        public int? AccountId { get; set; }

        public virtual Account? Account { get; set; }
    }
}
