using System;
using System.Collections.Generic;

namespace Infrastructure.Models
{
    public partial class Category
    {
        public Category()
        {
            StartupCategories = new HashSet<StartupCategory>();
        }

        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public virtual ICollection<StartupCategory> StartupCategories { get; set; }
    }
}
