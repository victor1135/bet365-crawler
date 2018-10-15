using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Felix.Bet365.NETCore.Crawler.Configuration;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using PuppeteerSharp.Mobile;

namespace Felix.Bet365.NETCore.Crawler.Engine
{
    public class PuppeteerEngine : IBaseEngine
    {

        private AppSettings _settings;
        private Browser _browser;
        public PuppeteerEngine(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
            launchBrowser();
        }


        public async Task<Page> newPage()
        {
            var page = await _browser.NewPageAsync();
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

            return page;
        }

        public async void launchBrowser(){
            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                ExecutablePath = _settings.ChromePath,
                Headless = false
            });
        }


    }
}
