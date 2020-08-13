using System;
using System.Collections.Generic;

namespace CommonLib.Extensions
{
    public static class EnumExtension
    {
        public static List<T> GetEnumList<T>() where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T必须是枚举类型。");

            List<T> ret = new List<T>();
            foreach (T item in Enum.GetValues(typeof(T)))
            {
                ret.Add(item);
            }
            return ret;
        }
    }
}
