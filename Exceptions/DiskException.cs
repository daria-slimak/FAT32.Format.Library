using System;

namespace FAT32.Format.Library
{
    public class DiskException : Exception
    {
        public DiskException() { }
        public DiskException(string message) : base(message) { }
    }
}
