using System;
using System.Linq;

namespace CommonLib.Extensions
{
    public static class ByteExtension
    {
        /// <summary>
        /// 将byte数组的子数组转换成Int32类型
        /// </summary>
        /// <param name="byteArray"></param>
        /// <param name="startIndex"></param>
        /// <param name="length">因Int32类型占4个字节，因此length不能大于4。</param>
        /// <returns></returns>
        public static int ToInt32(this byte[] byteArray, int startIndex, int length)
        {
            if (length > 4)
            {
                throw new Exception("length必须小于4");
            }

            byte[] toCopy = new byte[4];
            Array.Copy(byteArray, startIndex, toCopy, 4 - length, length);
            Array.Reverse(toCopy);
            return BitConverter.ToInt32(toCopy, 0);
        }

        /// <summary>
        /// 按照高位在前，低位在后的顺序将字节序列转换为Int32,
        /// 采用Span不涉及内存分配，速度很快
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static int ToInt32(this Span<byte> span)
        {
            if (span.Length > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(span));
            }
            
            byte byte1 = (byte)(span.Length > 3 ? span[0] : 0x00);  //高位
            byte byte2 = (byte)(span.Length > 3 ? span[1] : (span.Length > 2 ? span[0] : 0x00));
            byte byte3 = (byte)(span.Length > 3 ? span[2] : (span.Length > 2 ? span[1] : (span.Length > 1 ? span[0] : 0x00)));
            byte byte4 = (byte)(span.Length > 3 ? span[3] : (span.Length > 2 ? span[2] : (span.Length > 1 ? span[1] : (span.Length > 0 ? span[0] : 0x00))));  //低位

            return ((byte1 & 0xff) << 24)
                | ((byte2 & 0xff) << 16)
                | ((byte3 & 0xff) << 8)
                | (byte4 & 0xff);
        }

        /// <summary>
        /// 将16进制数组转换为相应字符串形式。
        /// </summary>
        /// <param name="byteArray">16进制数组</param>
        /// <param name="seperator">分隔符，默认为空格</param>
        /// <returns></returns>
        public static string ToHexString(this byte[] byteArray) => BitConverter.ToString(byteArray);

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
                    {
                        crc = (ushort)((crc >> 1) ^ 0xA001);
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return BitConverter.GetBytes(crc);
        }

        /// <summary>
        /// 默认获取整个数组的CRC16校验码
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        public static byte[] GetCRC16ByDefault(this byte[] byteArray)
        {
            return GetCRC16(byteArray, byteArray.Length);
        }

        /// <summary>
        /// 默认将数组最后两位当作校验位，进行CRC16校验。
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        public static bool CheckCRC16ByDefault(this byte[] byteArray)
        {
            if (byteArray == null)
            {
                return false;
            }

            if (byteArray.Length <= 2)
            {
                return false;
            }

            int length = byteArray.Length;
            byte[] crc = byteArray.GetCRC16(length - 2);

            return crc[0] == byteArray[length - 2] && crc[1] == byteArray[length - 1];
        }

        public static byte[] ToHex(this int value, bool reverse = true)
        {
            return reverse ? BitConverter.GetBytes(value).Reverse().ToArray() : BitConverter.GetBytes(value);
        }
    }
}
