using System;

namespace FAT32.Format.Library
{
    internal class FileSystem
    {
        /*
         * 
         * FAT32 Structure
         * 
         * **************************************
         * ***** Reserved Area
         * *** Start - 0
         * *** Size  - BPB_RsvdSecCnt
         * 
         * Sector 0 = BootSector
         * Sector 1 = FSInfo
         * Sector BPB_BkBootSec = BootSector Backup
         * Sector BPB_BkBootSec + 1 = FSInfo Backup
         * 
         * **************************************
         * 
         * **************************************
         * ***** FAT #1 Area
         * *** Start - BPB_RsvdSecCnt
         * *** Size  - BPB_FATSz32
         * **************************************
         * 
         * **************************************
         * ***** FAT #2 Area
         * *** Start - BPB_RsvdSecCnt + BPB_FATSz32
         * *** Size  - BPB_FATSz32
         * **************************************
         * 
         */

        private Disk _disk;
        private BootSector _bootSector;
        private FSInfoSector _fsInfoSector;

        public FileSystem(Disk disk)
        {
            _disk = disk;

            if (_disk.TotalSectors > 0xffffffff)
                throw new FileSystemException("Too many sectors! Max. is 4 294 967 295");

            _bootSector = new BootSector((int)_disk.BytesPerSector);
            _fsInfoSector = new FSInfoSector((int)_disk.BytesPerSector);
        }

        public void Format()
        {
            _bootSector.BytesPerSector = (ushort)_disk.BytesPerSector;
            _bootSector.SectorsPerCluster = SectorsPerCluster();
            _bootSector.SectorsPerTrack = (ushort)_disk.SectorsPerTrack;
            _bootSector.NumberOfHeads = (ushort)_disk.NumberOfHeads;
            _bootSector.HiddenSectors = (ushort)_disk.HiddenSectors;
            _bootSector.TotalSectors = (uint)_disk.TotalSectors;
            _bootSector.FATsize = FATsize();
            _bootSector.VolumeID = VolumeID();

            uint firstDataSector = _bootSector.ReservedSectorsCount + (_bootSector.NumberOfFATs * _bootSector.FATsize);
            uint dataSectors = _bootSector.TotalSectors - firstDataSector;
            uint countOfClusters = dataSectors / _bootSector.SectorsPerCluster;

            if (countOfClusters < (65525 + 16)) // Min. clusters + recommended offset
                throw new FileSystemException("Not enough clusters! Min. is 65 541");


            /*
             * http://elm-chan.org/docs/fat_e.html
             * 
             * "The FAT entry of FAT32 volume occupies 32 bits, but its upper 4 bits are reserved, only lower 28 bits are valid"
             * 
             * Cluster 0x00000000 = Free
             * Cluster 0x00000001 = Reserved
             * Cluster 0x0FFFFFF7 = Bad
             * Cluster 0x0FFFFFF[8 - F] = EOC
             * 
             */

            if (countOfClusters > 0x0FFFFFF4)
                throw new FileSystemException("Too many clusters! Max. is 268 435 444");

            _fsInfoSector.FreeClusters = countOfClusters;

            // Build sectors
            _bootSector.Build();
            _fsInfoSector.Build();

            // Zeros sectors
            Buffer buffer = new Buffer((int)_disk.BytesPerSector * 32); // BytesPerSectors + Burst

            uint sectorsToZeros = firstDataSector + _bootSector.SectorsPerCluster; // + Root dir cluster

            for (int sector = 0; sector < (sectorsToZeros + 32); sector += 32)
            {
                _disk.WriteSector(sector, buffer, (int)_disk.BytesPerSector * 32);
            }

            // Write BootSector and FSInfo
            _disk.WriteSector(0, _bootSector.Data, _bootSector.Size);
            _disk.WriteSector(1, _fsInfoSector.Data, _fsInfoSector.Size);

            // Write backups
            _disk.WriteSector(_bootSector.BackupSectorNumber, _bootSector.Data, _bootSector.Size);
            _disk.WriteSector(_bootSector.BackupSectorNumber + 1, _fsInfoSector.Data, _fsInfoSector.Size);

            // Write FAT entries
            buffer.Add(0x0FFFFFF8, 0); // BPB_Media
            buffer.Add(0x0FFFFFFF, 4); // Error history
            buffer.Add(0x0FFFFFFF, 8); // EOC of root dir

            int fatOffset = _bootSector.ReservedSectorsCount;

            // FAT #1
            _disk.WriteSector(fatOffset, buffer, _disk.BytesPerSector);

            // FAT #2
            _disk.WriteSector(fatOffset + (int)_bootSector.FATsize, buffer, _disk.BytesPerSector);
        }

        private byte SectorsPerCluster()
        {
            /*
             * 
             * https://support.microsoft.com/en-us/topic/default-cluster-size-for-ntfs-fat-and-exfat-9772e6f1-e31a-00d7-e18f-73169155af95
             * 
             */

            long GB = 1024 * 1024 * 1024;
            long MB = 1024 * 1024;
            int KB = 1024;

            // Over 32GB
            if (_disk.TotalSize > 32 * GB)
                return (byte)(32 * KB / _disk.BytesPerSector);

            // 16GB - 32GB
            if (_disk.TotalSize > 16 * GB)
                return (byte)(16 * KB / _disk.BytesPerSector);

            // 8GB - 16GB
            if (_disk.TotalSize > 8 * GB)
                return (byte)(8 * KB / _disk.BytesPerSector);

            // 256MB - 8GB
            if (_disk.TotalSize > 256 * MB)
                return (byte)(4 * KB / _disk.BytesPerSector);

            // 128MB - 256MB
            if (_disk.TotalSize > 128 * MB)
                return (byte)(2 * KB / _disk.BytesPerSector);

            // 64MB - 128MB
            if (_disk.TotalSize > 64 * MB)
                return (byte)(1 * KB / _disk.BytesPerSector);

            // 32MB - 64MB
            return 1;
        }

        private uint FATsize()
        {
            /*
             * 
             * fatgen103.doc - page 21
             * 
             */

            uint numerator = _bootSector.TotalSectors - _bootSector.ReservedSectorsCount;
            uint denominator = (uint)((256 * _bootSector.SectorsPerCluster) + _bootSector.NumberOfFATs) / 2;

            return (numerator + (denominator - 1)) / denominator;
        }

        private uint VolumeID()
        {
            DateTime dateTime = DateTime.Now;

            uint year = (uint)dateTime.Year - 1980;
            uint month = (uint)dateTime.Month;
            uint day = (uint)dateTime.Day;

            uint hour = (uint)dateTime.Hour;
            uint minute = (uint)dateTime.Minute;
            uint second = (uint)dateTime.Second / 2;

            return (year << 25) | (month << 21) | (day << 16) | (hour << 11) | (minute << 5) | second;
        }
    }
}
