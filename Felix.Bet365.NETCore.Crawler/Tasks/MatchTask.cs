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
using PuppeteerSharp.Mobile;
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
    public class MatchTask : BaseTask
    {
        private Func<XPathHandler, XPathHandler> _DateFilter = element => element.Find(TagEnum.Div, "@class='podHeaderRow'")
                                                                                        .Find(TagEnum.Div, "@class='wideLeftColumn'");
        private Func<XPathHandler, XPathHandler> _totalMatchValueFilter = element => element.Find(TagEnum.Div, "@data-fixtureid");

        private Func<XPathHandler, XPathHandler> _matchCompetitor = element => element.Find(TagEnum.Span, "@class='ippg-Market_Truncator'");

        private Func<XPathHandler, XPathHandler> _matchDate = element => element.Find(TagEnum.Div, "@class='ippg-Market_GameStartTime'");

        private HttpClientEngine _engine;

        private AppSettings _settings;

        public MatchTask(IBaseEngine engine, ILogger<CatergoryTask> logger, IOptions<AppSettings> settings, IServiceScopeFactory services)
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
                var leagues = raceDB.League.Where(x => x.Status == 1).ToList();
                foreach (var league in leagues)
                {
                    var totalMatchHtml = await GetMatchListAsync(league.LeagueKey);//await _engine.LoadHtml(string.Format(totalMatchUrl, league.LeagueKey.Trim()), JobTimeout);
                    var rawDate = HtmlHandler.GetImplement("Date", totalMatchHtml).Get(_DateFilter) + " 2018";
                    var attrs = HtmlHandler.GetImplement("TotalMatchAttrs", totalMatchHtml).GetsAttributes(_totalMatchValueFilter);
                    var values = HtmlHandler.GetImplement("TotalLeagueValues", totalMatchHtml).Gets(_totalMatchValueFilter);

                    if (values == null)
                    {
                        continue;
                    }
                    var matchDataList = (from a in attrs
                                         from b in values
                                         where a.Key == b.Key
                                         where a.Value.Any(x => x.Key == "data-fixtureid")
                                         select new
                                         {
                                             MatchKey = a.Value.Where(x => x.Key == "data-fixtureid").FirstOrDefault().Value,
                                             MatchValue = b.Value
                                         });

                    foreach (var match in matchDataList)
                    {
                        var matchData = HtmlHandler.GetImplement("matchData", match.MatchValue).Gets(_matchCompetitor);
                        var matchDate = HtmlHandler.GetImplement("matchDate", match.MatchValue).Get(_matchDate);
                        var gameStartDate = new DateTimeOffset(Convert.ToDateTime(matchDate + " " + rawDate), new TimeSpan(-4, 0, 0));

                        RaceDB.Models.Match matchModel = new RaceDB.Models.Match();
                        var existMatch = raceDB.Match.Where(x => x.MatchKey == match.MatchKey && x.StartDateTime == gameStartDate).FirstOrDefault();
                        if (existMatch != null)
                        {
                            matchModel = existMatch;
                        }

                        var homeCompetitor = matchData[0];
                        var awayCompetitor = matchData[1];

                        matchModel.MatchKey = match.MatchKey;
                        matchModel.LeagueId = league.LeagueId;
                        matchModel.CategoryId = league.CategoryId;
                        matchModel.HomeCompetitorName = homeCompetitor;
                        matchModel.AwayCompetitorName = awayCompetitor;
                        matchModel.Status = 0;
                        matchModel.InPlay = false;
                        matchModel.SportId = 0;
                        matchModel.StartDateTime = gameStartDate;
                        matchModel.CreateDate = DateTimeOffset.Now.ToOffset(new TimeSpan(-4, 0, 0));
                        matchModel.UpdateDate = DateTimeOffset.Now.ToOffset(new TimeSpan(-4, 0, 0));
                        matchModel.ResultStatus = 0;

                        if (existMatch == null)
                        {
                            raceDB.Add(matchModel);
                        }
                        raceDB.SaveChanges();
                    }
                }
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

        public async Task<string> GetMatchListAsync(string leagueKey){
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                ExecutablePath = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe",
                Headless = false
            });

            var page = await browser.NewPageAsync();
            DeviceDescriptor IPhone = DeviceDescriptors.Get(DeviceDescriptorName.IPhone6);
            var dic = new Dictionary<string, string>();
            dic.Add("Referer", _settings.Bet365.Url.MainPage.ToString());
            dic.Add("Accept-Encoding", "gzip, deflate, br");
            dic.Add("Accept-Language", "zh-TW,zh;q=0.9,en-US;q=0.8,en;q=0.7,zh-CN;q=0.6");
            dic.Add("Connection", "keep-alive");
            await page.EmulateAsync(IPhone);
            await page.SetRequestInterceptionAsync(true);
            await page.SetExtraHttpHeadersAsync(dic);

            page.Request += async (sender, e) =>
            {
                if (e.Request.ResourceType == ResourceType.Image)
                    await e.Request.AbortAsync();
                else
                    await e.Request.ContinueAsync();
            };
            var waitUntil = new NavigationOptions();
            waitUntil.WaitUntil = new WaitUntilNavigation[1];
            waitUntil.WaitUntil.Append(WaitUntilNavigation.Networkidle2);
            await page.GoToAsync(_settings.Bet365.Url.MainPage.ToString(), waitUntil);
            var waitOption = new WaitForSelectorOptions
            {
                Timeout = 10000,
                Hidden = true
            };
            var preLoadOuter = await page.WaitForXPathAsync(_settings.Bet365.ElementXpath.PreLoader, waitOption);
            waitOption.Hidden = false;
            var selectSport = await page.WaitForXPathAsync(_settings.Bet365.ElementXpath.Soccer, waitOption);
            await selectSport.ClickAsync();

            var selectLeague = await page.WaitForXPathAsync("//*[@data-sportskey='"+leagueKey.Trim()+"']", waitOption);
            await selectLeague.ClickAsync();
            //Thread.Sleep(2000);
            var selectMatch = await page.WaitForXPathAsync("//*[@data-fixtureid]", waitOption);
            // await selectMatch.ClickAsync();
            return await page.GetContentAsync();
        }


    }
}
