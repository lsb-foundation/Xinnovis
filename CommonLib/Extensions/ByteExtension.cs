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
    }
}
