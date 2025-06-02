using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class AccountProfile
    {
        public int AccountProfileId { get; set; }
        public int? AccountId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }
        public DateTime? Dob { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string? BackgroundUrl { get; set; }

        public virtual Account? Account { get; set; }
    }
}
