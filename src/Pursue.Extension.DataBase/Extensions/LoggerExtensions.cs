using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pursue.Extension.DataBase.DependencyInjection;
using Pursue.Extension.DataBase.HiveMap;
using System;

namespace Pursue.Extension.DataBase
{
    static class Log
    {
        private static readonly ILogger _logger;

        static Log()
        {
            _logger = DataBaseDependencyInjection.ServiceProvider.GetService<ILogger<HiveMapsHandler>>();
        }

        public static void DeBug(string message, params object?[] args)
        {
            _logger.LogDebug(message, args);
        }

        public static void Error(Exception ex)
        {
            _logger.LogError(ex, "");
        }
    }
}
