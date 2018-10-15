using Felix.Bet365.NETCore.Crawler.Configuration;
using Felix.Bet365.NETCore.Crawler.Engine;
using Felix.Bet365.NETCore.Crawler.Enum;
using Felix.Bet365.NETCore.Crawler.Handler;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Felix.Bet365.NETCore.Crawler.Tasks
{
    [DisallowConcurrentExecution]
    public class CatergoryTask : BaseTask
    {
        private Func<XPathHandler, XPathHandler> _elementFilter = element => element.Find(TagEnum.Div, "@data-nav='couponLink'");
        private Func<XPathHandler, XPathHandler> _totalCounrtyFilter = element => element.Find(TagEnum.H3, "@data-sportskey");

        private PuppeteerEngine _engine;

        private AppSettings _settings;

        public CatergoryTask(PuppeteerEngine engine, ILogger<CatergoryTask> logger, IOptions<AppSettings> settings, IServiceScopeFactory services)
        {
            serviceScopeFactory = services;
            _engine = engine;
            _logger = logger;
            _settings = settings.Value;
        }

        protected override async Task TaskAsync(CancellationToken cancellationToken)
        {
            await this.ParseAsync(cancellationToken);
            throw new NotImplementedException();
        }

        public override async Task<object> ParseAsync(CancellationToken cancellationToken)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {

                var raceDB = scope.ServiceProvider.GetService<RaceDB.Models.RaceDBContext>();
                var page  = await _engine.newPage();
                await page.GoToAsync(_settings.Bet365.Url.MainPage.ToString());
                var waitOption = new WaitForSelectorOptions
                {
                    Timeout = 30000,
                    Hidden = true
                };
                var preLoadOuter = await page.WaitForXPathAsync(_settings.Bet365.ElementXpath.PreLoader, waitOption);
                waitOption.Hidden = false;
                var selectSport = await page.WaitForXPathAsync(_settings.Bet365.ElementXpath.Soccer, waitOption);
                Thread.Sleep(3000);
                await selectSport.ClickAsync();
                Thread.Sleep(3000);
                
                var totalCatergoryHtml =  await page.GetContentAsync(); 
                var attrs = HtmlHandler.GetImplement("TotalCatergory", totalCatergoryHtml).GetsAttributes(_totalCounrtyFilter);
                var values = HtmlHandler.GetImplement("TotalCatergory", totalCatergoryHtml).Gets(_totalCounrtyFilter);
                var catergoryData = (from a in attrs
                                    from b in values
                                    where a.Key == b.Key
                                    select new
                                    {
                                        CatergoryKey = a.Value.Where(x => x.Key == "data-sportskey").FirstOrDefault().Value,
                                        CatergoryValue = b.Value
                                    });

                var differentDatas = catergoryData.Where(x => raceDB.Category.Any(g => g.CategoryName == x.CatergoryValue && g.CategoryKey != x.CatergoryKey)).ToList();

                foreach (var data in differentDatas)
                {
                    var category = raceDB.Category.Where(x => x.CategoryName == data.CatergoryValue).FirstOrDefault();
                    category.CategoryKey = data.CatergoryKey;
                }
                await raceDB.SaveChangesAsync();

        
            }
            return null;
        }

        protected override int JobTimeout
        {
            get
            {
                return 300000;
            }
        }


    }
}
