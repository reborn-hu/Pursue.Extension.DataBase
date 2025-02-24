namespace Pursue.Extension.DataBase
{
    public interface IRepositoryFactory
    {
        IRepository<TEntity> GetReadRepository<TEntity>(DbBusinessType businessType) where TEntity : class, new();

        IRepository<TEntity> GetWriteRepository<TEntity>(DbBusinessType businessType) where TEntity : class, new();

        TRepository GetRepository<TRepository>();
    }
}