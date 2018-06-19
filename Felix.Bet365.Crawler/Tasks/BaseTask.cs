using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Felix.Bet365.Crawler.Tasks
{
    public abstract class BaseTask : IJob
    {

        protected abstract System.Threading.Tasks.Task TaskAsync(CancellationToken cancellationToken);

         Task IJob.Execute(IJobExecutionContext context)
        {
            var cancelTokenSource = new CancellationTokenSource();
            return Task.Run(async () => await this.TaskAsync(cancelTokenSource.Token), cancelTokenSource.Token);
        }

        protected ILog Logger
        {
            get
            {
                return LogManager.GetLogger(this.TaskName);
            }
        }

        public virtual string TaskName
        {
            get { return GetType().Name; }
        }

        protected abstract int JobTimeout { get; }

        ~BaseTask()
        {
        }
    }
}
