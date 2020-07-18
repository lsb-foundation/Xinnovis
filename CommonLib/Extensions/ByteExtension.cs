using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Extensions
{
    public static class ByteExtension
    {
        public static int ToInt32(this byte[] byteArray, int startIndex, int length)
        {
            if (length > 4)
                throw new Exception("length必须小于4");
            if (startIndex + length > byteArray.Length)
                throw new IndexOutOfRangeException();

            byte[] toCopy = new byte[4] { 0, 0, 0, 0 };
            Array.Copy(byteArray, startIndex, toCopy, 4 - length, length);
            return BitConverter.ToInt32(toCopy.Reverse().ToArray(), 0);
        }

        /// <summary>
        /// 将16进制数组转换为相应字符串形式。
        /// </summary>
        /// <param name="byteArray">16进制数组</param>
        /// <param name="seperator">分隔符，默认为空格</param>
        /// <returns></returns>
        public static string ToHexString(this byte[] byteArray, string seperator = " ")
        {
            string hex = BitConverter.ToString(byteArray);
            return hex.Replace("-", seperator);
        }
    }
}
