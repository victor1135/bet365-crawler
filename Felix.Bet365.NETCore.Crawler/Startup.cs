using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Felix.Bet365.NETCore.Crawler.Configuration;
using Felix.Bet365.NETCore.Crawler.Engine;
using Felix.Bet365.NETCore.Crawler.Engine.Http;
using Felix.Bet365.NETCore.Crawler.Job;
using Felix.Bet365.NETCore.Crawler.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl;
using RedisConfig;

namespace Felix.Bet365.NETCore.Crawler
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration);
            services.AddDbContext<RaceDB.Models.RaceDBContext>(options =>
            options.UseSqlServer(this.Configuration.GetConnectionString("DefaultConnection")));
            services.AddSingleton(GetSettings());
            services.AddLogging();
            services.AddMvc();
            services.AddQuartz();
            services.Configure<RedisConfiguration>(Configuration.GetSection("redis"));

            services.AddDistributedRedisCache(options =>
            {
                options.InstanceName = Configuration.GetValue<string>("redis:name");
                options.Configuration = Configuration.GetValue<string>("redis:host");
            });

            services.AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();
            //services.AddTransient<CatergoryTask>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,IApplicationLifetime lifetime)
        {
            //var quartz = new QuartzStartup(app.ApplicationServices,loggerFactory.CreateLogger<QuartzStartup>());
            //quartz.Start();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseQuartz();
            app.UseMvc();
        }

        private IConfigurationRoot GetSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Configuration"))
                .AddJsonFile(path: "Settings.json", optional: true, reloadOnChange: true);
            return builder.Build();
        }

    }
}
