using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CommonLib.Extensions
{
    public static class StringExtension
    {
        public static int HexNumber(this char aChar) =>
            aChar switch
            {
                char c when c >= '0' && c <= '9' => c - '0',
                char c when c == 'a' || c == 'A' => 0x0A,
                char c when c == 'b' || c == 'B' => 0x0B,
                char c when c == 'c' || c == 'C' => 0x0C,
                char c when c == 'd' || c == 'D' => 0x0D,
                char c when c == 'e' || c == 'E' => 0x0E,
                char c when c == 'f' || c == 'F' => 0x0F,
                _ => throw new ArgumentOutOfRangeException(nameof(aChar))
            };

        public static byte[] HexStringToBytes(this string hex)
        {
            Stack<int> stack = new();
            char current, next = '\0';
            for (int index = hex.Length; index > 0; index--)
            {
                current = hex[index - 1];
                if (current == ' ') continue;
                if (next != '\0')
                {
                    stack.Push(current.HexNumber() * 16 + next.HexNumber());
                    next = '\0';
                }
                else next = current;
            }
            if (next != '\0') stack.Push(next.HexNumber());

            int count = stack.Count;
            byte[] bytes = new byte[count];
            for (int index = 0; index < count; index++)
            {
                bytes[index] = (byte)stack.Pop();
            }
            return bytes;
        }

        public static string MD5HashString(this string text)
        {
            return BitConverter.ToString(
                new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(text)))
                .Replace("-", string.Empty);
        }
    }
}
