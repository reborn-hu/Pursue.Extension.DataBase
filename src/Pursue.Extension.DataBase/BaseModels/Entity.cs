using Pursue.Extension.DataBase.SnowflakeId;
using SqlSugar;
using System;

namespace Pursue.Extension.DataBase
{
    /// <summary>
    /// 基础实体对象
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// 自增Id
        /// </summary>
        [SugarColumn(ColumnName = "id", SqlParameterDbType = System.Data.DbType.Int64, IsPrimaryKey = true, IsIdentity = false)]
        public long Id { get; set; }

        /// <summary>
        /// 有效标识  1:有效 0:无效
        /// </summary>
        [SugarColumn(ColumnName = "active", SqlParameterDbType = System.Data.DbType.Boolean)]
        public bool Active { get; set; } = true;

        /// <summary>
        /// 删除标识  1:删除 0:未删除
        /// </summary>
        [SugarColumn(ColumnName = "del", SqlParameterDbType = System.Data.DbType.Boolean)]
        public bool Del { get; set; } = false;

        /// <summary>
        /// 创建人
        /// </summary>
        [SugarColumn(ColumnName = "create_user", SqlParameterDbType = System.Data.DbType.AnsiString, Length = 50)]
        public string CreateUser { get; set; } = "";

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnName = "create_time", SqlParameterDbType = System.Data.DbType.DateTime, IsNullable = true)]
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        [SugarColumn(ColumnName = "modify_user", SqlParameterDbType = System.Data.DbType.AnsiString, Length = 50)]
        public string ModifyUser { get; set; } = "";

        /// <summary>
        /// 操作时间
        /// </summary>
        [SugarColumn(ColumnName = "modify_time", SqlParameterDbType = System.Data.DbType.DateTime, IsNullable = true)]
        public DateTime? ModifyTime { get; set; }
    }

    public static class EntityOperate
    {
        public static TEntity Create<TEntity>(this TEntity entity, string createUser) where TEntity : Entity
        {
            entity.Id = SnowflakeFactory.Id;
            entity.Del = false;
            entity.Active = true;
            entity.CreateUser = createUser;
            entity.CreateTime = SystemTime.Now;

            return entity;
        }

        public static TEntity Create<TEntity>(this TEntity entity, long id, string createUser) where TEntity : Entity
        {
            entity.Id = id;
            entity.Del = false;
            entity.Active = true;
            entity.CreateUser = createUser;
            entity.CreateTime = SystemTime.Now;

            return entity;
        }


        public static TEntity Modify<TEntity>(this TEntity entity, string modifyUser) where TEntity : Entity
        {
            entity.ModifyUser = modifyUser;
            entity.ModifyTime = SystemTime.Now;

            return entity;
        }

        public static TEntity Modify<TEntity>(this TEntity entity, long id, string modifyUser) where TEntity : Entity
        {
            entity.Id = id;
            entity.ModifyUser = modifyUser;
            entity.ModifyTime = SystemTime.Now;

            return entity;
        }
    }
}