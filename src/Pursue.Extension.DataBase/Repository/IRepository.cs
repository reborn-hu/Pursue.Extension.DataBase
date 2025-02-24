using SqlSugar;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pursue.Extension.DataBase
{
    public interface IRepository<TEntity> : ISimpleClient<TEntity> where TEntity : class, new()
    {
        IRepository<TEntity> CreateRepository(ISqlSugarClient sqlSugarClient);

        List<TEntity> Query(string sql, int commandTimeOut = 60);
        List<TEntity> Query(string sql, int commandTimeOut = 60, params object[] parameter);
        List<TEntity> Query<TParam>(string sql, TParam parameter, int commandTimeOut = 60);
        Task<List<TEntity>> QueryAsync(string sql, int commandTimeOut = 60);
        Task<List<TEntity>> QueryAsync(string sql, int commandTimeOut = 60, params object[] parameter);
        Task<List<TEntity>> QueryAsync<TParam>(string sql, TParam parameter, int commandTimeOut = 60);
        TEntity QuerySingle(string sql, int commandTimeOut = 60);
        TEntity QuerySingle(string sql, int commandTimeOut = 60, params object[] parameter);
        TEntity QuerySingle<TParam>(string sql, TParam parameter, int commandTimeOut = 60);
        Task<TEntity> QuerySingleAsync(string sql, int commandTimeOut = 60);
        Task<TEntity> QuerySingleAsync(string sql, int commandTimeOut = 60, params object[] parameter);
        Task<TEntity> QuerySingleAsync<TParam>(string sql, TParam parameter, int commandTimeOut = 60);
    }
}