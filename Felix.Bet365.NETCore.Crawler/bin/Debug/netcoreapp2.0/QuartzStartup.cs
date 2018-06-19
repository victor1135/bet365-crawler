using Felix.Bet365.NETCore.Crawler.Engine;
using Felix.Bet365.NETCore.Crawler.Engine.Http;
using Felix.Bet365.NETCore.Crawler.Job;
using Felix.Bet365.NETCore.Crawler.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Logging;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Felix.Bet365.NETCore.Crawler
{
    //public class QuartzStartup
    //{
    //    private IScheduler _scheduler;
    //    private ILogger _logger;
    //    public IServiceProvider Services { get; }

    //    public QuartzStartup(IServiceProvider services,ILogger<QuartzStartup> logger)
    //    {
    //        Services = services;
    //        _logger = logger;
    //    }

    //    public  void Start()
    //    {
    //        DoWork();
    //    }

    //    private void DoWork()
    //    {
    //        _logger.LogInformation("Consume Scoped Service Hosted Service is working.");
    //        var jobFactory = new JobFactory(Services);
    //        using (var scope = Services.CreateScope())
    //        {
    //            var factory =
    //                scope.ServiceProvider
    //                    .GetRequiredService<ISchedulerFactory>();

    //            if (_scheduler != null)
    //            {
    //                throw new InvalidOperationException("Already started.");
    //            }
    //            LogProvider.SetCurrentLogProvider(new ConsoleLogProvider(_logger));
    //            _scheduler = factory.GetScheduler().GetAwaiter().GetResult();
    //            _scheduler.JobFactory = jobFactory;
    //            _scheduler.Start().Wait();
    //        }
    //    }


    //    public void Stop()
    //    {
    //        if (_scheduler == null)
    //        {
    //        }

    //        // give running jobs 30 sec (for example) to stop gracefully
    //        if (_scheduler.Shutdown(waitForJobsToComplete: true).Wait(30000))
    //        {
    //            _scheduler = null;
    //        }
    //        else
    //        {
    //            // jobs didn't exit in timely fashion - log a warning...
                
    //        }
    //    }

    //    private class ConsoleLogProvider : ILogProvider
    //    {
    //        private ILogger _logger;

    //        public ConsoleLogProvider(ILogger logger)
    //        {
    //            _logger = logger;
    //        }

    //        public Logger GetLogger(string name)
    //        {
    //            return (level, func, exception, parameters) =>
    //            {
    //                if(func != null && level >= Quartz.Logging.LogLevel.Info)
    //                {
    //                    _logger.LogInformation("[Quartz-" + level + "] " + func(), parameters);
    //                }
    //                return true;
    //            };
    //        }

    //        public IDisposable OpenNestedContext(string message)
    //        {
    //            throw new NotImplementedException();
    //        }

    //        public IDisposable OpenMappedContext(string key, string value)
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }
    //}


    public static class QuartzExtensions
    {
        public static async void UseQuartz(this IApplicationBuilder app)
        {
            await app.ApplicationServices.GetService<Task<IScheduler>>();
        }

        public static void AddQuartz(this IServiceCollection services)
        {
            services.AddSingleton<ILogProvider, ConsoleLogProvider>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            //services.AddTransient<IBaseEngine, PhantomJsCloud>();
            services.AddTransient<IBaseEngine, HttpClientEngine>();
            services.AddTransient<IJobFactory, JobFactory>();
            services.AddTransient<CatergoryTask>();
            services.AddTransient<LeagueTask>();
            services.AddTransient<MatchTask>();
            services.AddSingleton(async provider =>
            {
                var consoleLog = provider.GetService<ILogProvider>();
                var schedulerFactory = provider.GetService<ISchedulerFactory>();
                LogProvider.SetCurrentLogProvider(consoleLog);
                var scheduler =await schedulerFactory.GetScheduler();
                scheduler.JobFactory = provider.GetService<IJobFactory>();
                await scheduler.Start();
                return scheduler;
            });
        }


    }

    




}
