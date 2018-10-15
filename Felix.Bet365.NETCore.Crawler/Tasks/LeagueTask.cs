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
    public class LeagueTask : BaseTask
    {
        private Func<XPathHandler, XPathHandler> _totalLeagueFilter = element => element.Find(TagEnum.Div, "@data-sportskey");
        private Func<XPathHandler, XPathHandler> _totalLeagueValueFilter = element => element.Find(TagEnum.Div, "@data-sportskey")
                                                                                        .Find(TagEnum.Span);
        private PuppeteerEngine _engine;

        private AppSettings _settings;

        public LeagueTask(PuppeteerEngine engine, ILogger<CatergoryTask> logger, IOptions<AppSettings> settings, IServiceScopeFactory services)
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
                var categoryKeys = raceDB.Category.Where(x => x.Status == 1).ToList();


                foreach (var key in categoryKeys)
                {
                    Func<XPathHandler, XPathHandler> categoryFilter = element => element.Find(TagEnum.Div, "@class='eventWrapper cc_34_8'");
                    var page = await _engine.newPage();
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
                    var totalLeagueHtml = await page.GetContentAsync();
                    var html = HtmlHandler.GetImplement("TotalLeague", totalLeagueHtml).Gets(categoryFilter)[0];
                    var attrs = HtmlHandler.GetImplement("TotalLeague", html).GetsAttributes(_totalLeagueFilter);
                    var values = HtmlHandler.GetImplement("TotalLeague", html).Gets(_totalLeagueValueFilter);
                    var leagueData = (from a in attrs
                                      from b in values
                                      where a.Key == b.Key
                                      select new
                                      {
                                          LeagueKey = a.Value.Where(x => x.Key == "data-sportskey").FirstOrDefault().Value,
                                          LeagueValue = b.Value
                                      });
                    var differentDatas = leagueData.Where(x => raceDB.League.Any(g => g.LeagueName != x.LeagueValue && g.LeagueKey != x.LeagueKey)).ToList();
                    foreach (var data in differentDatas)
                    {
                        var league = raceDB.League.Where(x => x.LeagueName == data.LeagueValue).FirstOrDefault();
                        if (league != null)
                        {
                            league.LeagueKey = data.LeagueKey;
                        }
                        else
                        {
                            RaceDB.Models.League leagueModel = new RaceDB.Models.League();
                            leagueModel.CategoryId = key.CategoryId;
                            leagueModel.LeagueKey = data.LeagueKey;
                            leagueModel.LeagueName = data.LeagueValue;
                            raceDB.Add(leagueModel);
                        }
                        //.LeagueKey = data.LeagueKey;

                    }
                    await raceDB.SaveChangesAsync();

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


    }
}
