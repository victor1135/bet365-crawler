using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Felix.Bet365.NETCore.Crawler.Model
{
    public class Selection
    {
        public string BetTypeSN { get; set; }

        public string BetTypeNM { get; set; }

        public List<BetFieldType> BetFieldList { get; set; }

    }

    public class BetFieldType
    {
        public string BetFieldTypeSN { get; set; }
        public string BetFieldTypeNM { get; set; }
        public string Odds { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }

}