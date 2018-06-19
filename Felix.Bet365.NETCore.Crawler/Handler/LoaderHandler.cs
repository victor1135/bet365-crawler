
using Felix.Bet365.NETCore.Crawler.Enum;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Felix.Bet365.NETCore.Crawler.Handler
{
    public class LoadHandler
    {
        protected string _logPrefix;

        private IDictionary<string, TaskModel> _tasks;

        private CancellationToken _cancellationToken;

        protected ILogger _logger;

        public LoadHandler(ILogger<LoadHandler> logger)
        {
            _logger = logger;
        }

        public void Implement(string logPrefix, CancellationToken cancellationToken)
        {
            this._logPrefix = logPrefix;
            this._cancellationToken = cancellationToken;
            this._tasks = new Dictionary<string, TaskModel>();
        }

        protected class TaskModel
        {
            public CrawlerEngingEnum ParserEnging { get; set; }
            public string Url { get; set; }
            public int Retry { get; set; }
            public int HttpTimeout { get; set; }
            //public Func<HtmlHandler, string, bool> Filter { get; set; }
        }

        public LoadHandler Add(string key, CrawlerEngingEnum parserEnging, string url, int retry, int httpTimeout)
        {
            this._tasks.Add(key, new TaskModel
            {
                ParserEnging = parserEnging,
                Url = url,
                Retry = retry,
                HttpTimeout = httpTimeout,
            });
            return this;
        }

        public IDictionary<string, string> WaitAll()
        {
            IDictionary<string, Task<string>> taskArray = new Dictionary<string, Task<string>>();
            foreach (var item in _tasks)
            {
                taskArray.Add(item.Key, GetHtmlAsync(item.Key, item.Value.Retry));
            }
            Task.WaitAll(taskArray.Select(a => a.Value).ToArray());
            return taskArray.ToDictionary(a => a.Key, a => a.Value.Result);
        }

        public async Task<string> GetHtmlAsync(string key, int retry)
        {
            var taskData = _tasks[key];
            var webHandler = WebHandler.GetImplement(_logPrefix, taskData.ParserEnging, _proxy);

            Task task = Task.Run(() => webHandler.LoadHtml(taskData.Url, taskData.HttpTimeout), _cancellationToken);

            Logger.Info($"URL[{taskData.Url}] Parser Start. Retry[{taskData.Retry - retry}] use VPS[{webHandler.ProxyName}]");
            if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(taskData.HttpTimeout - 2), _cancellationToken)) != task)
            {
                Logger.Info($"URL[{taskData.Url}] Parser Failure, because timeout. Retry[{taskData.Retry - retry}] use VPS[{webHandler.ProxyName}]");
                webHandler.Cancel();
            }

            var content = webHandler.Content;

            var parserResult = !string.IsNullOrEmpty(content);

            if (parserResult && taskData.Filter != null)
            {
                parserResult = taskData.Filter(HtmlHandler.GetImplement(_logPrefix, content), content);
            }

            if (webHandler.IsLoadSuccess == LoadStatusEnum.Success && parserResult)
            {
                Logger.Info($"URL[{taskData.Url}] Parser Success. Retry[{taskData.Retry - retry}] use VPS[{webHandler.ProxyName}]");
                return content;
            }
            else
            {
                if (webHandler.IsLoadSuccess == LoadStatusEnum.Success && !parserResult)
                {
                    Logger.Info($"URL[{taskData.Url}] Parser Success. but content filter failure. Retry[{taskData.Retry - retry}] use VPS[{webHandler.ProxyName}]");
                }
                if (retry > 0)
                {
                    await Task.Delay(1500);
                    return await GetHtmlAsync(key, (retry - 1));
                }
                else
                {
                    Logger.Info($"URL[{taskData.Url}] Parser Failure. use VPS[{webHandler.ProxyName}]");
                    return "";
                }
            }
        }
    }
}
