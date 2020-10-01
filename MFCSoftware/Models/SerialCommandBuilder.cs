using CommonLib.Extensions;
using System;
using System.Collections.Generic;

namespace MFCSoftware.Models
{
    public class SerialCommandBuilder
    {
        private readonly List<byte> serialBytes = new List<byte>();

        public SerialCommandType Type { get; }

        public SerialCommandBuilder(SerialCommandType type)
        {
            Type = type;
        }

        public SerialCommandBuilder AppendAddress(int addr)
        {
            serialBytes.Add(Convert.ToByte(addr));
            return this;
        }

        public SerialCommandBuilder AppendBytes(byte data)
        {
            serialBytes.Add(data);
            return this;
        }

        public SerialCommandBuilder AppendBytes(byte[] bytes)
        {
            serialBytes.AddRange(bytes);
            return this;
        }

        public SerialCommandBuilder AppendCrc16()
        {
            byte[] crc = serialBytes.ToArray().GetCRC16ByDefault();
            serialBytes.AddRange(crc);
            return this;
        }

        public byte[] ToArray()
        {
            return serialBytes.ToArray();
        }

        public SerialCommand<byte[]> ToSerialCommand(int responseLength)
        {
            return new SerialCommand<byte[]>(serialBytes.ToArray(), Type, responseLength);
        }
    }
}
