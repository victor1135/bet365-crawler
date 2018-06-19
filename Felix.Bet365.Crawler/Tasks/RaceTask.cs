using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Felix.Bet365.Crawler.Tasks
{
    [DisallowConcurrentExecution]
    public  class RaceTask : BaseTask
    {

        protected override Task TaskAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override int JobTimeout
        {
            get
            {
                return 1000;
            }
        }
    }
}
