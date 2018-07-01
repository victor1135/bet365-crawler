using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Felix.Bet365.NETCore.Crawler.Configuration
{
    public class AppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; }
        public ElementXpath ElementXpath { get; set; }
        public Bet365 Bet365 { get; set; }
        public string ChromePath { get; set; }
    }

    public class ConnectionStrings
    {
        public string DefaultConnection { get; set; }
    }

    public class Bet365
    {
        public Url Url { get; set; }
        public ElementXpath ElementXpath { get; set; }
    }

    public class ElementXpath
    {
        public string PreLoader { get; set; }
        public string Soccer { get; set; }
    }

    public class Url
    {
        public string MainPage { get;set; }
        public string TotalCatergoryUrl { get; set; }
        public string TotalLeagueUrl { get; set; }
        public string CatergoryUrl { get; set; }
        public string TotalMatchUrl { get; set; }
        public string RaceDetailUrl { get; set; }
    }

}
