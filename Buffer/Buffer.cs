using System;
using System.Text;

namespace FAT32.Format.Library
{
    internal class Buffer
    {
        private byte[] _data;

        public Buffer(int size)
        {
            _data = new byte[size];
        }

        public byte[] Get()
        {
            return _data;
        }

        public void Add(byte[] array, int offset, int count)
        {
            System.Buffer.BlockCopy(array, 0, _data, offset, count);
        }

        public void Add(string str, int offset)
        {
            System.Buffer.BlockCopy(Encoding.ASCII.GetBytes(str), 0, _data, offset, str.Length);
        }

        public void Add(byte value, int offset)
        {
            _data[offset] = value;
        }

        public void Add(ushort value, int offset)
        {
            Add(BitConverter.GetBytes(value), offset, sizeof(ushort));
        }

        public void Add(uint value, int offset)
        {
            Add(BitConverter.GetBytes(value), offset, sizeof(uint));
        }
    }
}
