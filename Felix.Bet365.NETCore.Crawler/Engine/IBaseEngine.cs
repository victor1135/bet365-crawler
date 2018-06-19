using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Felix.Bet365.NETCore.Crawler.Engine
{
    public abstract class IBaseEngine
    {
        public virtual Task<string> LoadHtml() {
            return null;
        }
    }


}
