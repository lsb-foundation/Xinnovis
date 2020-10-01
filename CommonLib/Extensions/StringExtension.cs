using System.Globalization;

namespace CommonLib.Extensions
{
    public static class StringExtension
    {
        public static byte[] HexStringToBytes(this string hex)
        {
            hex = hex.Replace(" ", string.Empty);
            int len = ((hex.Length + 1) / 2) * 2;
            hex = hex.PadLeft(len, '0');

            var bytes = new byte[len / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                var currentHex = hex.Substring(i, 2);
                var hexByte = byte.Parse(currentHex, NumberStyles.HexNumber);
                bytes[i / 2] = hexByte;
            }
            return bytes;
        }
    }
}
