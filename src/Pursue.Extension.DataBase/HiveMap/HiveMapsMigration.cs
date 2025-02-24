using FluentMigrator;
using Mapster;
using SqlSugar;

namespace Pursue.Extension.DataBase.HiveMap
{
    [SugarTable("hivemaps_settings")]
    public sealed class HiveMapsEntity : Entity
    {
        /// <summary>
        /// 租户编码
        /// </summary>
        [SugarColumn(ColumnName = "tenant_id", SqlParameterDbType = System.Data.DbType.Int64)]
        public string TenantId { get; set; }

        /// <summary>
        /// 分库编码
        /// </summary>
        [SugarColumn(ColumnName = "hivemaps_code", SqlParameterDbType = System.Data.DbType.AnsiString, SqlParameterSize = 100)]
        public string HiveMapsCode { get; set; }

        /// <summary>
        /// 数据库业务类型
        /// </summary>
        [SugarColumn(ColumnName = "business_type")]
        public DbBusinessType BusinessType { get; set; }

        /// <summary>
        /// 写库连接字符串
        /// </summary>
        [SugarColumn(ColumnName = "connect_write", SqlParameterDbType = System.Data.DbType.AnsiString, IsNullable = false)]
        public string WriteConnectString { get; set; }

        /// <summary>
        /// 读库连接字符串
        /// </summary>
        [SugarColumn(ColumnName = "connect_read", SqlParameterDbType = System.Data.DbType.AnsiString, IsNullable = false)]
        public string ReadConnectString { get; set; }
    }

    public sealed class HiveMapsModel
    {
        /// <summary>
        /// 租户编码
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 数据库业务类型
        /// </summary>
        public DbBusinessType Type { get; set; }

        /// <summary>
        /// 写库连接字符串
        /// </summary>
        public string Write { get; set; }

        /// <summary>
        /// 读库连接字符串
        /// </summary>
        public string Read { get; set; }
    }

    [Migration(5270170002125028)]
    public class HiveMapsMigration : Migration
    {
        public override void Up()
        {
            Create
              .Table("hivemaps_settings")
              .WithColumn("id").AsInt64().PrimaryKey().WithColumnDescription("业务Id")

              .WithColumn("tenant_id").AsAnsiString(100).WithColumnDescription("租户编码")
              .WithColumn("hivemaps_code").AsAnsiString(20).WithColumnDescription("分库编码")
              .WithColumn("business_type").AsByte().WithColumnDescription("数据库业务类型")
              .WithColumn("connect_write").AsAnsiString(20).WithColumnDescription("写库连接字符串")
              .WithColumn("connect_read").AsAnsiString(100).WithColumnDescription("读库连接字符串")

              .WithColumn("enabled").AsBoolean().WithDefaultValue(true).WithColumnDescription(" 启用标识")
              .WithColumn("del").AsBoolean().WithDefaultValue(false).WithColumnDescription("删除标识")
              .WithColumn("create_user").AsAnsiString(64).WithDefaultValue("").WithColumnDescription("创建人")
              .WithColumn("create_date").AsDateTime().WithDefaultValue("1970-01-01").WithColumnDescription("创建时间")
              .WithColumn("modify_user").AsAnsiString(64).WithDefaultValue("").WithColumnDescription("操作人")
              .WithColumn("modify_date").AsDateTime().WithDefaultValue("1970-01-01").WithColumnDescription("操作时间");
        }

        public override void Down()
        {
            Delete.Table("hivemaps_settings");
        }
    }

    public sealed class HiveMapsMapperRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config
                .ForType<HiveMapsEntity, HiveMapsModel>()
                .Map(e => e.Id, i => i.TenantId)
                .Map(e => e.Type, i => i.BusinessType)
                .Map(e => e.Write, i => i.WriteConnectString.DecodeConnectString())
                .Map(e => e.Read, i => i.ReadConnectString.DecodeConnectString());
        }
    }
}
