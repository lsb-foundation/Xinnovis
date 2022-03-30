using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonLib.Extensions
{
    public static class EnumExtension
    {
        public static List<T> GetEnumList<T>() where T : struct
            => typeof(T).IsEnum switch
            {
                true => Enum.GetValues(typeof(T)).OfType<T>().ToList(),
                false => throw new ArgumentException("类型T必须是枚举")
            };
    }
}
