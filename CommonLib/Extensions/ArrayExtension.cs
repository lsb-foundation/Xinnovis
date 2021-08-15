using System;

namespace CommonLib.Extensions
{
    public static class ArrayExtension
    {
        /// <summary>
        /// 获取特定长度的子数组
        /// </summary>
        /// <param name="array"></param>
        /// <param name="startIndex">开始位置的索引</param>
        /// <param name="length">子数组的长度</param>
        /// <returns></returns>
        public static T[] SubArray<T>(this T[] array, int startIndex, int length)
            => array.AsSpan().Slice(startIndex, length).ToArray();
    }
}
