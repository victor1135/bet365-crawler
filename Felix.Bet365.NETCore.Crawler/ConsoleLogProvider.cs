using System;
using Microsoft.Extensions.Logging;
using Quartz.Logging;

namespace Felix.Bet365.NETCore.Crawler
{
    internal class ConsoleLogProvider : ILogProvider
    {
        private ILogger _logger;

        public ConsoleLogProvider(ILoggerFactory factory)
        {
            _logger = factory.CreateLogger("ConsoleLogProvider");
        }

        public Logger GetLogger(string name)
        {
            return (level, func, exception, parameters) =>
            {
                if (func != null && level >= Quartz.Logging.LogLevel.Info)
                {
                    _logger.LogInformation("[Quartz-" + level + "] " + func(), parameters);
                }
                return true;
            };
        }

        public IDisposable OpenNestedContext(string message)
        {
            throw new NotImplementedException();
        }

        public IDisposable OpenMappedContext(string key, string value)
        {
            throw new NotImplementedException();
        }
    }
}