using NodaTime;
using System;

namespace Pursue.Extension.DataBase
{
    /// <summary>
    /// 时间获取扩展
    /// </summary>
    static class SystemTime
    {
        /// <summary>
        /// 获取东八区当前时间 (+ 8:00)
        /// </summary>
        /// <returns></returns>
        public static DateTime Now => SystemClock.Instance.GetCurrentInstant().InZone(DateTimeZoneProviders.Tzdb["Asia/Shanghai"]).ToDateTimeUnspecified();

        /// <summary>
        /// 根据时区获取当前时间
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public static DateTime GetNow(string zone) => SystemClock.Instance.GetCurrentInstant().InZone(DateTimeZoneProviders.Tzdb[zone]).ToDateTimeUnspecified();
    }
}