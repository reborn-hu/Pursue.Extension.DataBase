using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pursue.Extension.DataBase.HiveMap;
using Pursue.Extension.DataBase.SnowflakeId;
using SqlSugar;
using System;

namespace Pursue.Extension.DataBase.DependencyInjection
{
    public static class DataBaseDependencyInjection
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        public static IServiceCollection AddDataBase(this IServiceCollection services, Action<ConnectionOptions> options)
        {
            if (options == default)
            {
                throw new NullReferenceException("ORM DataBase 配置文件不可为空!");
            }

            options.Invoke(new ConnectionOptions());

            if (string.IsNullOrWhiteSpace(ConnectionOptions.BaseConnectString))
            {
                throw new NullReferenceException("未搜索到基础数据连接，无法正常启动应用程序!请配置BaseConnectString!");
            }

            services.TryAddSingleton<ISqlSugarClient>(option => { return ORM.SqlSugar; });
            services.TryAddSingleton<HiveMapsHandler>();
            services.TryAddSingleton<IRepositoryFactory, RepositoryFactory>();
            services.TryAddTransient(typeof(IRepository<>), typeof(Repository<>));

            services.TryAddSingleton<ISnowflake, Snowflake>();

            if (ConnectionOptions.MigratorEnable)
            {
                services.AddFluentMigratorCore().ConfigureRunner(options =>
                {
                    switch (ConnectionOptions.DbType)
                    {
                        case DbType.Sqlite:
                            options.AddSQLite();
                            break;
                        case DbType.PostgreSQL:
                            options.AddPostgres();
                            break;
                        case DbType.SqlServer:
                            options.AddSqlServer();
                            break;
                        case DbType.MySql:
                            options.AddMySql5();
                            break;
                    }
                    options.WithGlobalConnectionString(ConnectionOptions.MigratorConnectString);
                    options.ScanIn(ConnectionOptions.MigratorType.Assembly).For.Migrations();
                })
                .AddLogging(options => options.AddFluentMigratorConsole());
            }

            ServiceProvider = services.BuildServiceProvider();

            return services;
        }
    }
}