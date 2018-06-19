using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Felix.Bet365.NETCore.Crawler.Engine
{
    public class HttpClientEngine : IBaseEngine
    {
        private HttpClient engine { get; set; }

        public HttpClientEngine()
        {
            engine = new HttpClient();
        }
        

        public async Task<string> LoadHtml(string url,int timeout)
        {
            var  cts = new CancellationTokenSource(timeout);
            var respone = await engine.GetAsync(url, cts.Token);
            return await respone.Content.ReadAsStringAsync();
        }


    }
}
