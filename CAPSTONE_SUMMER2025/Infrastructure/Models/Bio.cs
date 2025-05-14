using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class Bio
    {
        public int BioId { get; set; }
        public int? AccountId { get; set; }
        public string? IntroTitle { get; set; }
        public string? Cvlink { get; set; }
        public string? Position { get; set; }
        public string? Workplace { get; set; }
        public string? FacebookUrl { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? GithubUrl { get; set; }
        public string? PortfolioUrl { get; set; }
        public string? Country { get; set; }
        public string? PersonalWebsiteUrl { get; set; }
        public bool? IsPublicProfile { get; set; }

        public virtual Account? Account { get; set; }
    }
}
