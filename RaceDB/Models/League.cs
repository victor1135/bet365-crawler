using System;
using System.Collections.Generic;

namespace RaceDB.Models
{
    public partial class League
    {
        public League()
        {
            Match = new HashSet<Match>();
        }

        public int LeagueId { get; set; }
        public int CategoryId { get; set; }
        public string LeagueName { get; set; }
        public string LeagueKey { get; set; }
        public int Status { get; set; }

        public Category Category { get; set; }
        public ICollection<Match> Match { get; set; }
    }
}
