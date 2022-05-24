using System;

namespace FAT32.Format.Library
{
    public class FileSystemException : Exception
    {
        public FileSystemException() { }
        public FileSystemException(string message) : base(message) { }
    }
}
