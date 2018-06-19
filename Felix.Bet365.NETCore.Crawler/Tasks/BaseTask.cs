
using Felix.Bet365.NETCore.Crawler.Engine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Felix.Bet365.NETCore.Crawler.Tasks
{
    public abstract class BaseTask : IJob
    {
        protected abstract System.Threading.Tasks.Task TaskAsync(CancellationToken cancellationToken);

        Task IJob.Execute(IJobExecutionContext context)
        {
            var cancelTokenSource = new CancellationTokenSource();
            return Task.Run(async () => await this.TaskAsync(cancelTokenSource.Token), cancelTokenSource.Token);
        }

        public abstract Task<object> ParseAsync(CancellationToken cancellationToken);

        protected ILogger _logger;

        protected IServiceScopeFactory serviceScopeFactory;

       // protected IBaseEngine _engine;

        protected IConfiguration _config;


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
