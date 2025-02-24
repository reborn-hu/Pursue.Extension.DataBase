using Microsoft.Extensions.DependencyInjection;
using Pursue.Extension.DataBase.DependencyInjection;
using Pursue.Extension.DataBase.HiveMap;
using System;

namespace Pursue.Extension.DataBase
{
    public sealed class RepositoryFactory : IRepositoryFactory
    {
        private readonly HiveMapsHandler _hiveMaps;
        private readonly TenantContext _tenant;

        public RepositoryFactory(HiveMapsHandler hiveMaps, TenantContext tenant)
        {
            _hiveMaps = hiveMaps;
            _tenant = tenant;

        }

        /// <summary>
        /// 获取写库
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="businessType"></param>
        /// <returns></returns>
        public IRepository<TEntity> GetWriteRepository<TEntity>(DbBusinessType businessType) where TEntity : class, new()
        {
            var tenantId = GetTenantId(businessType);

            var connect = _hiveMaps.GetConnectString(tenantId, businessType, DbCommadType.Write);

            var orm = ORM.SwitchDbContext(tenantId, businessType, connect);

            var repository = DataBaseDependencyInjection.ServiceProvider.GetService<IRepository<TEntity>>().CreateRepository(orm);

            return repository;
        }

        /// <summary>
        /// 获取读库
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="businessType"></param>
        /// <returns></returns>
        public IRepository<TEntity> GetReadRepository<TEntity>(DbBusinessType businessType) where TEntity : class, new()
        {
            var tenantId = GetTenantId(businessType);

            var connect = _hiveMaps.GetConnectString(tenantId, businessType, DbCommadType.Read);

            var orm = ORM.SwitchDbContext(tenantId, businessType, connect);

            var repository = DataBaseDependencyInjection.ServiceProvider.GetService<IRepository<TEntity>>().CreateRepository(orm);

            return repository;
        }

        /// <summary>
        /// 获取自定义应用数据仓储
        /// </summary>
        /// <typeparam name="TRepository"></typeparam>
        /// <returns></returns>
        public TRepository GetRepository<TRepository>()
        {
            try
            {
                if (DataBaseDependencyInjection.ServiceProvider.GetService(typeof(TRepository)) is TRepository repository)
                {
                    return repository;
                }
                throw new Exception();
            }
            catch
            {
                var tenantId = GetTenantId(DbBusinessType.Base);
                throw new NullReferenceException($"TenantId:{tenantId},Not obtained hivemaps !");
            }
        }

        private string GetTenantId(DbBusinessType businessType)
        {
            if (ConnectionOptions.ShardingEnable)
            {
                return _tenant.TenantId;
            }
            else
            {
                return businessType.GetDescription();
            }
        }
    }
}