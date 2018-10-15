using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Felix.Bet365.NETCore.Crawler.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using RedisConfig;

namespace Felix.Bet365.NETCore.Crawler.Controllers
{

    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly RedisConfiguration _redis;
        private readonly IRedisConnectionFactory _fact;

        public ValuesController(IOptions<RedisConfiguration> redis, IRedisConnectionFactory factory){
            _redis = redis.Value;
            _fact = factory;
        }
        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<string>> GetAsync()
        {

            var db = _fact.Connection().GetDatabase();
            var redis = new RedisVoteService<Selection>(this._fact);
            
            var selection = new Selection();
            selection.BetTypeSN = "1101";
            selection.BetTypeNM = "Match Result";
            
            var betFields = new List<BetFieldType>();
            betFields.Add(new BetFieldType{
                BetFieldTypeSN = "1",
                BetFieldTypeNM = "1",
                Odds = "1.2"
            });
            betFields.Add(new BetFieldType
            {
                BetFieldTypeSN = "2",
                BetFieldTypeNM = "2",
                Odds = "2.1"
            });
            betFields.Add(new BetFieldType
            {
                BetFieldTypeSN = "3",
                BetFieldTypeNM = "Draw",
                Odds = "1.8"
            });

            selection.BetFieldList = betFields;
            redis.Save("RedisVote:Black", selection);
            
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
