using CommonLib.Extensions;
using System;
using System.Collections.Generic;

namespace MFCSoftware.Models
{
    public class SerialCommandBuilder
    {
        private List<byte> serialBytes = new List<byte>();

        public SerialCommandBuilder AppendAddress(int addr)
        {
            serialBytes.Add(Convert.ToByte(addr));
            return this;
        }

        public SerialCommandBuilder AppendBytes(byte[] bytes)
        {
            serialBytes.AddRange(bytes);
            return this;
        }

        public SerialCommandBuilder AppendCrc16()
        {
            byte[] crc = serialBytes.ToArray().GetCRC16(serialBytes.Count);
            serialBytes.AddRange(crc);
            return this;
        }

        public SerialCommand<byte[]> ToSerialCommand(int returnedLength)
        {
            return new SerialCommand<byte[]>(serialBytes.ToArray(), returnedLength);
        }
    }
}
