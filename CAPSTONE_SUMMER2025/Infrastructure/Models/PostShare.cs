using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class PostShare
    {
        public int PostShareId { get; set; }
        public int? AccountId { get; set; }
        public int? PostId { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreateAt { get; set; }

        public virtual Account? Account { get; set; }
        public virtual Post? Post { get; set; }
    }
}
