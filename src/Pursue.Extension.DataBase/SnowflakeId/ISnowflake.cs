namespace Pursue.Extension.DataBase.SnowflakeId
{
    public interface ISnowflake
    {
        long GetId();

        string GetIdToString();
    }
}