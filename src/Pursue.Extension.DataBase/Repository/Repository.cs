using SqlSugar;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pursue.Extension.DataBase
{
    public class Repository<TEntity> : SimpleClient<TEntity>, IRepository<TEntity> where TEntity : class, new()
    {
        public IRepository<TEntity> CreateRepository(ISqlSugarClient sqlSugarClient)
        {
            Context = sqlSugarClient;

            return this;
        }

        public TEntity QuerySingle(string sql, int commandTimeOut = 60)
        {
            Context.Ado.CommandTimeOut = commandTimeOut;

            return Context.Ado.SqlQuerySingle<TEntity>(sql);
        }

        public TEntity QuerySingle(string sql, int commandTimeOut = 60, params object[] parameter)
        {
            Context.Ado.CommandTimeOut = commandTimeOut;

            return Context.Ado.SqlQuerySingle<TEntity>(sql, parameter);
        }

        public TEntity QuerySingle<TParam>(string sql, TParam parameter, int commandTimeOut = 60)
        {
            if (parameter == null)
                return default;

            var parameters = ORM.Convert.ToParameter(sql, parameter);

            Context.Ado.CommandTimeOut = commandTimeOut;

            return Context.Ado.SqlQuerySingle<TEntity>(sql, parameters);
        }


        public List<TEntity> Query(string sql, int commandTimeOut = 60)
        {
            Context.Ado.CommandTimeOut = commandTimeOut;

            return Context.Ado.SqlQuery<TEntity>(sql);
        }

        public List<TEntity> Query(string sql, int commandTimeOut = 60, params object[] parameter)
        {
            Context.Ado.CommandTimeOut = commandTimeOut;

            return Context.Ado.SqlQuery<TEntity>(sql, parameter);
        }

        public List<TEntity> Query<TParam>(string sql, TParam parameter, int commandTimeOut = 60)
        {
            if (parameter == null)
                return default;

            var parameters = ORM.Convert.ToParameter(sql, parameter);

            Context.Ado.CommandTimeOut = commandTimeOut;

            return Context.Ado.SqlQuery<TEntity>(sql, parameters);
        }


        public async Task<List<TEntity>> QueryAsync(string sql, int commandTimeOut = 60)
        {
            Context.Ado.CommandTimeOut = commandTimeOut;

            return await Context.Ado.SqlQueryAsync<TEntity>(sql);
        }

        public async Task<List<TEntity>> QueryAsync(string sql, int commandTimeOut = 60, params object[] parameter)
        {
            Context.Ado.CommandTimeOut = commandTimeOut;

            return await Context.Ado.SqlQueryAsync<TEntity>(sql, parameter);
        }

        public async Task<List<TEntity>> QueryAsync<TParam>(string sql, TParam parameter, int commandTimeOut = 60)
        {
            if (parameter == null)
                return default;

            var parameters = ORM.Convert.ToParameter(sql, parameter);

            Context.Ado.CommandTimeOut = commandTimeOut;

            return await Context.Ado.SqlQueryAsync<TEntity>(sql, parameters);
        }


        public async Task<TEntity> QuerySingleAsync(string sql, int commandTimeOut = 60)
        {
            Context.Ado.CommandTimeOut = commandTimeOut;

            return await Context.Ado.SqlQuerySingleAsync<TEntity>(sql);
        }

        public async Task<TEntity> QuerySingleAsync(string sql, int commandTimeOut = 60, params object[] parameter)
        {
            Context.Ado.CommandTimeOut = commandTimeOut;

            return await Context.Ado.SqlQuerySingleAsync<TEntity>(sql, parameter);
        }

        public async Task<TEntity> QuerySingleAsync<TParam>(string sql, TParam parameter, int commandTimeOut = 60)
        {
            if (parameter == null)
                return default;

            var parameters = ORM.Convert.ToParameter(sql, parameter);

            Context.Ado.CommandTimeOut = commandTimeOut;

            return await Context.Ado.SqlQuerySingleAsync<TEntity>(sql, parameters);
        }
    }
}