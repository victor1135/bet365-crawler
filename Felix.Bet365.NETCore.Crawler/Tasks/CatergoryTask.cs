using Felix.Bet365.NETCore.Crawler.Configuration;
using Felix.Bet365.NETCore.Crawler.Engine;
using Felix.Bet365.NETCore.Crawler.Enum;
using Felix.Bet365.NETCore.Crawler.Handler;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

        private HttpClientEngine _engine;

        private AppSettings _settings;

        public CatergoryTask(IBaseEngine engine, ILogger<CatergoryTask> logger, IOptions<AppSettings> settings, IServiceScopeFactory services)
        {
            serviceScopeFactory = services;
            _engine = (HttpClientEngine)engine;
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
                var totalCatergoryUrl = _settings.Bet365.Url.TotalCatergoryUrl;
                var totalCatergoryHtml = await _engine.LoadHtml(totalCatergoryUrl, JobTimeout);
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

                //foreach (var countryKey in countryKeys)
                //{

                //var raceUrl = _config["Bet365Url:CountryUrl"];
                //var data = await _engine.LoadHtml(string.Format(raceUrl, countryKey), JobTimeout);
                //var attrs = HtmlHandler.GetImplement("Country", data).GetsAttributes(_elementFilter);
                //foreach (var attr in attrs)
                //{
                //    var sport_key = attr.Value["data-sportskey"];
                //    var totalRaceUrl = _config["Bet365Url:TotalRaceUrl"];
                //    var raceDetailUrl = _config["Bet365Url:RaceDetailUrl"];
                //    var raceHtmlData = await _engine.LoadHtml(string.Format(totalRaceUrl, attr.Value["data-sportskey"]), JobTimeout);

                //    Func<XPathHandler, XPathHandler> _elementFilter2 = element => element.Find(TagEnum.Div, $"@class='liveAlertKey enhancedPod cc_65_5 forceopen'")
                //                                                                         .Find(TagEnum.Div, "@data-fixtureid");

                //    var totalRaceData = HtmlHandler.GetImplement("TotalRace", raceHtmlData).GetsAttributes(_elementFilter2);
                //    var qq = totalRaceData.FirstOrDefault().Value["data-nav"].Split(',')[2];

                //    var raceDetailHtmlData = await _engine.LoadHtml(string.Format(raceDetailUrl, qq), JobTimeout);
                //}
                //}

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
