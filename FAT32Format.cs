using System;

namespace FAT32.Format.Library
{
    public class FAT32Format
    {
        private Disk _disk;
        private FileSystem _fs;

        public FAT32Format(char driveLetter)
        {
            if (!char.IsLetter(driveLetter))
                throw new ArgumentException("Drive letter is not a letter");

            _disk = new Disk(driveLetter);
        }

        public void Format()
        {
            _fs = new FileSystem(_disk);
            _fs.Format();
            _disk.Dispose();
        }
    }
}
