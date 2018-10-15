using Felix.Bet365.NETCore.Crawler.Configuration;
using Felix.Bet365.NETCore.Crawler.Engine;
using Felix.Bet365.NETCore.Crawler.Enum;
using Felix.Bet365.NETCore.Crawler.Handler;
using Felix.Bet365.NETCore.Crawler.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using PuppeteerSharp.Mobile;
using Quartz;
using RedisConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using RaceDB.Models;

namespace Felix.Bet365.NETCore.Crawler.Tasks
{
    [DisallowConcurrentExecution]
    public class OddsTask : BaseTask
    {
        private PuppeteerEngine _engine;
        private readonly RedisConfiguration _redis;
        private readonly IRedisConnectionFactory _fact;
        private AppSettings _settings;
        
        public OddsTask(PuppeteerEngine engine, ILogger<CatergoryTask> logger, IOptions<AppSettings> settings, IServiceScopeFactory services, IOptions<RedisConfiguration> redis, IRedisConnectionFactory factory)
        {
            serviceScopeFactory = services;
            _logger = logger;
            _settings = settings.Value;
            _redis = redis.Value;
            _fact = factory;
            _engine = engine;
        }

        protected override async Task TaskAsync(CancellationToken cancellationToken)
        {
            await this.ParseAsync(cancellationToken);
            throw new NotImplementedException();
        }

        public override async Task<object> ParseAsync(CancellationToken cancellationToken)
        {
            //string contents = File.ReadAllText(@"E:\365.txt");
            //SaveOddsToRedis(contents);
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var raceDB = scope.ServiceProvider.GetService<RaceDB.Models.RaceDBContext>();
                var dateTimeFilter = DateTimeOffset.Now.AddHours(-7);
                var matches = raceDB.Match.AsQueryable().Include(x => x.League).Include(x => x.Category).Where(x => x.Status == 2).Take(3).ToList();
                var redis = new RedisVoteService<MatchInfo>(this._fact);
                
                foreach (var match in matches){
                    if(redis.Get($"{match.StartDateTime.ToString("yyyyMMdd")}:{match.MatchId}") == null){
                        var matchInfo = new MatchInfo
                        {
                            MatchId = match.MatchId,
                            CategoryName = match.Category.CategoryName,
                            LeagueName = match.League.LeagueName,
                            SportName = "Soccer",
                            StartDateTime = match.StartDateTime,
                            Status = match.Status,
                            HomeCompetitorName = match.HomeCompetitorName,
                            AwayCompetitorName = match.AwayCompetitorName
                        };
                        redis.Save($"{match.StartDateTime.ToString("yyyyMMdd")}:{match.MatchId}", matchInfo);
                    }
                    
                    var page = await _engine.newPage();
                    var targetPage = await GetMatchOddsAsync(page, match);
                    Task.Run(async  () =>  {
                        do
                        {
                            await Task.Delay(3000);
                            var qq = await targetPage.GetContentAsync();
                            SaveOddsToRedis(match, qq);
                            await Task.Delay(3000);
                        } while (true);
                    });
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

        public async Task<Page> GetMatchOddsAsync(Page page, Match match){
            var waitUntil = new NavigationOptions();
            waitUntil.WaitUntil = new WaitUntilNavigation[1];
            waitUntil.WaitUntil.Append(WaitUntilNavigation.Networkidle2);
            await page.GoToAsync(_settings.Bet365.Url.MainPage.ToString(), waitUntil);
            var waitOption = new WaitForSelectorOptions
            {
                Timeout = 30000,
                Hidden = true
            };
            var preLoadOuter = await page.WaitForXPathAsync(_settings.Bet365.ElementXpath.PreLoader, waitOption);
            waitOption.Hidden = false;
            var selectSport = await page.WaitForXPathAsync(_settings.Bet365.ElementXpath.Soccer, waitOption);
            await Task.Delay(6000);
            await selectSport.ClickAsync();
            
            var selectLeague = await page.WaitForXPathAsync("//*[@data-sportskey='"+ match.League.LeagueKey.Trim()+"']", waitOption);
            await Task.Delay(5000);
            await selectLeague.ClickAsync();
            // var selectLeagueList = await page.WaitForXPathAsync("//h3[@data-sportskey='" + leagueKey.Trim() + "']", waitOption);
            // Thread.Sleep(5000);
            // await selectLeagueList.ClickAsync();
            //Thread.Sleep(2000);
            var selectMatch = await page.WaitForXPathAsync("//*[@data-fixtureid='"+match.MatchKey.Trim()+ "']/div/div", waitOption);
            await Task.Delay(5000);
            await selectMatch.ClickAsync();
            return page;
            
            
        }

        private static void NewMethod()
        {
            Task.Delay(3000);
        }
        private Func<XPathHandler, XPathHandler> _fullTimeResultFilter = element => element.Find(TagEnum.EM, "contains(., 'Full Time Result')")
                                                                                        .Parent().Parent().Find(TagEnum.Div, "@data-nav").Find(TagEnum.Span, "@class='odds'");
        
                                                                                        
        public  void SaveOddsToRedis(Match match, string contents){

            var rawFullTimeResult = HtmlHandler.GetImplement("Date", contents).Gets(_fullTimeResultFilter);
            var date = DateTimeOffset.Now.ToString("yyyyMMdd");
            var timeStamp = DateTimeOffset.Now;
            var key = $"{date}:{match.MatchId}:odds:1101";


            var db = _fact.Connection().GetDatabase();
            var redis = new RedisVoteService<BetFieldType>(this._fact);
            if(rawFullTimeResult != null)
            {
                redis.Delete(key);
            }
            var selection = new Selection();
            selection.BetTypeSN = "1101";
            selection.BetTypeNM = "Match Result";

            var betFields = new List<BetFieldType>();
            betFields.Add(new BetFieldType
            {
                BetFieldTypeSN = "1",
                BetFieldTypeNM = "1",
                Odds = rawFullTimeResult[0],
                TimeStamp = timeStamp
            });
            betFields.Add(new BetFieldType
            {
                BetFieldTypeSN = "3",
                BetFieldTypeNM = "Draw",
                Odds = rawFullTimeResult[1],
                TimeStamp = timeStamp
            });
            betFields.Add(new BetFieldType
            {
                BetFieldTypeSN = "2",
                BetFieldTypeNM = "2",
                Odds = rawFullTimeResult[2],
                TimeStamp = timeStamp
            });
            
            selection.BetFieldList = betFields;
            redis.SaveList(key, betFields);
        }
    }
}
