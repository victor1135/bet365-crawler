using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Felix.Bet365.NETCore.Crawler.Model
{
    public class MatchInfo
    {
        public int MatchId { get; set; }
        public string SportName { get; set; }
        public string CategoryName { get; set; }
        public string LeagueName { get; set; }
        public int Status { get; set; }
        public DateTimeOffset StartDateTime { get; set; }
        public string HomeCompetitorName { get; set; }
        public string AwayCompetitorName { get; set; }

    }

}