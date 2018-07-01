using System;
using System.Collections.Generic;

namespace RaceDB.Models
{
    public partial class Category
    {
        public Category()
        {
            League = new HashSet<League>();
            Match = new HashSet<Match>();
        }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryKey { get; set; }
        public int Status { get; set; }

        public ICollection<League> League { get; set; }
        public ICollection<Match> Match { get; set; }
    }
}
