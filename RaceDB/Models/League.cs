using System;
using System.Collections.Generic;

namespace RaceDB.Models
{
    public partial class League
    {
        public int LeagueId { get; set; }
        public int CategoryId { get; set; }
        public string LeagueName { get; set; }
        public string LeagueKey { get; set; }
        public int Status { get; set; }
    }
}
