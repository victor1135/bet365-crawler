using System;
using System.Collections.Generic;

namespace RaceDB.Models
{
    public partial class Match
    {
        public int MatchId { get; set; }
        public int SportId { get; set; }
        public int CategoryId { get; set; }
        public int LeagueId { get; set; }
        public string MatchKey { get; set; }
        public bool InPlay { get; set; }
        public int Status { get; set; }
        public DateTimeOffset StartDateTime { get; set; }
        public int ResultStatus { get; set; }
        public string HomeCompetitorName { get; set; }
        public string AwayCompetitorName { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public DateTimeOffset UpdateDate { get; set; }

        public Category Category { get; set; }
        public League League { get; set; }
    }
}
