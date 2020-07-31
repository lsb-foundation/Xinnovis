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
        /// <summary>
        /// 将byte数组的子数组转换成Int32类型
        /// </summary>
        /// <param name="byteArray"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int ToInt32(this byte[] byteArray, int startIndex, int length)
        {
            if (length > 4)
                throw new Exception("length必须小于4");
            if (startIndex + length > byteArray.Length)
                throw new IndexOutOfRangeException();

            byte[] toCopy = new byte[4];
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

        /// <summary>
        /// 获取CRC16校验码
        /// </summary>
        /// <param name="byteArray"></param>
        /// <param name="length"></param>
        /// <returns>返回结果高位在前，低位在后。</returns>
        public static byte[] GetCRC16(this byte[] byteArray, int length)
        {
            ushort crc = 0xffff;
            for (int i = 0; i < length; i++)
            {
                crc ^= byteArray[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x0001) != 0)
                        crc = (ushort)((crc >> 1) ^ 0xA001);
                    else
                        crc >>= 1;
                }
            }
            return BitConverter.GetBytes(crc);
        }

        /// <summary>
        /// 默认将数组最后两位当作校验位，进行CRC16校验。
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        public static bool CheckCRC16ByDefault(this byte[] byteArray)
        {
            if (byteArray == null) return false;
            if (byteArray.Length <= 2) return false;

            int length = byteArray.Length;
            byte[] crc = byteArray.GetCRC16(length - 2);

            return crc[0] == byteArray[length - 2] && crc[1] == byteArray[length - 1];
        }
    }
}
