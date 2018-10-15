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
            services.AddSingleton<PuppeteerEngine>();
            services.AddTransient<IJobFactory, JobFactory>();
            services.AddTransient<CatergoryTask>();
            services.AddTransient<LeagueTask>();
            services.AddTransient<MatchTask>();
            services.AddTransient<OddsTask>();
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
