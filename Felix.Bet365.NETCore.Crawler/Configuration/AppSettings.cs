using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Felix.Bet365.NETCore.Crawler.Configuration
{
    public class AppSettings
    {
        public  ConnectionStrings ConnectionStrings { get; set; }
        public  Bet365Url Bet365Url { get; set; }
    }

    public class ConnectionStrings
    {
        public  string DefaultConnection { get; set; }
    }

    public class Bet365Url
    {

        public  string TotalCatergoryUrl { get; set; }
        public string TotalLeagueUrl { get; set; }
        public  string CatergoryUrl { get; set; }
        public  string TotalMatchUrl { get; set; }
        public  string RaceDetailUrl { get; set; }
    }

}
