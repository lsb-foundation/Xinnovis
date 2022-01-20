using CommonLib.Extensions;
using System;
using System.Collections.Generic;

namespace MFCSoftware.Utils
{
    public class SerialCommandBuilder
    {
        private readonly List<byte> serialBytes = new List<byte>();
        private readonly bool typeIsSetted;
        private int responseLength;

        public SerialCommandType Type { get; }  //可以不需要

        public SerialCommandBuilder()
        {
            typeIsSetted = false;
        }

        public SerialCommandBuilder(SerialCommandType type)
        {
            Type = type;
            typeIsSetted = true;
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

        public SerialCommandBuilder WithResponseLength(int len)
        {
            responseLength = len;
            return this;
        }

        public byte[] ToArray()
        {
            return serialBytes.ToArray();
        }

        public SerialCommand<byte[]> Build()
        {
            if(typeIsSetted)
                return new SerialCommand<byte[]>(serialBytes.ToArray(), Type, responseLength);
            return new SerialCommand<byte[]>(serialBytes.ToArray(), responseLength);
        }
    }
}
