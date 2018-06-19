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
                foreach(var league in leagues)
                {
                    var totalMatchUrl = _settings.Bet365Url.TotalMatchUrl;
                    var totalMatchHtml = await _engine.LoadHtml(string.Format(totalMatchUrl, league.LeagueKey.Trim()), JobTimeout);
                    var rawDate = HtmlHandler.GetImplement("Date", totalMatchHtml).Get(_DateFilter)+" 2018";
                    var attrs = HtmlHandler.GetImplement("TotalMatchAttrs", totalMatchHtml).GetsAttributes(_totalMatchValueFilter);
                    var values = HtmlHandler.GetImplement("TotalLeagueValues", totalMatchHtml).Gets(_totalMatchValueFilter);
                    if(values == null)
                    {
                        continue;
                    }
                    var matchDataList = (from a in attrs
                                      from b in values
                                      where a.Key == b.Key
                                      select new
                                      {
                                          MatchKey = a.Value.Where(x => x.Key == "data-nav").FirstOrDefault().Value.Split(',')[2],
                                          MatchValue = b.Value
                                      });
                    
                    foreach(var match in matchDataList)
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


    }
}
