using System;
using System.ComponentModel;
using System.Reflection;

namespace Pursue.Extension.DataBase
{
    /// <summary>
    /// 枚举操作扩展
    /// </summary>
    static class EnumExtension
    {
        /// <summary>获取枚举描述</summary>
        /// <param name="en">枚举</param>
        /// <returns>枚举的描述</returns>
        public static string GetDescription(this Enum en)
        {
            MemberInfo[] memInfo = en.GetType().GetMember(en.ToString());
            if (memInfo != null && memInfo.Length != 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length != 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return en.ToString();
        }
    }
}