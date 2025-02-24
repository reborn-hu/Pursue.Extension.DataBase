using Microsoft.Extensions.DependencyInjection;
using Pursue.Extension.DataBase.DependencyInjection;

namespace Pursue.Extension.DataBase.SnowflakeId
{
    public static class SnowflakeFactory
    {
        private static readonly ISnowflake _snowflake;

        static SnowflakeFactory()
        {
            _snowflake = DataBaseDependencyInjection.ServiceProvider.GetService<ISnowflake>();
        }

        public static long Id => _snowflake.GetId();

        public static string IdToString => _snowflake.GetIdToString();
    }
}