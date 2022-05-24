namespace FAT32.Format.Library
{
    internal class BootSector : Sector
    {
        /*
         * fatgen103.doc       Size : Offset
         * 
         * BS_jmpBoot             3 : 0 byte
         * BS_OEMName             8 : 3 byte
         * BPB_BytsPerSec         2 : 11 byte
         * BPB_SecPerClus         1 : 13 byte
         * BPB_RsvdSecCnt         2 : 14 byte
         * BPB_NumFATs            1 : 16 byte
         * BPB_RootEntCnt         2 : 17 byte
         * BPB_TotSec16           2 : 19 byte
         * BPB_Media              1 : 21 byte
         * BPB_FATSz16            2 : 22 byte
         * BPB_SecPerTrk          2 : 24 byte
         * BPB_NumHeads           2 : 26 byte
         * BPB_HiddSec            4 : 28 byte
         * BPB_TotSec32           4 : 32 byte
         * BPB_FATSz32            4 : 36 byte
         * BPB_ExtFlags           2 : 40 byte
         * BPB_FSVer              2 : 42 byte
         * BPB_RootClus           4 : 44 byte
         * BPB_FSInfo             2 : 48 byte
         * BPB_BkBootSec          2 : 50 byte
         * BPB_Reserved          12 : 52 byte
         * BS_DrvNum              1 : 64 byte
         * BS_Reserved1           1 : 65 byte
         * BS_BootSig             1 : 66 byte
         * BS_VolID               4 : 67 byte
         * BS_VolLab             11 : 71 byte
         * BS_FilSysType          8 : 82 byte
         * 
         * Checksum               2 : 510 byte AND (base.Size - 2) IF base.Size > 512 bytes
         * 
         */

        private readonly ushort RootEntriesCount = 0;
        private readonly ushort TotalSectors16 = 0;
        private readonly ushort FAT16sizeInSectors = 0;
        private readonly byte BootSignature = 0x29;
        private readonly string FileSystemType = "FAT32   ";
        private readonly byte[] Checksum = new byte[2] { 0x55, 0xAA };

        public byte[] JMPBoot { get; set; } = new byte[3] { 0xEB, 0x58, 0x90 };
        public string OEMName { get; set; } = "MSWIN4.1";
        public ushort BytesPerSector { get; set; }
        public byte SectorsPerCluster { get; set; }
        public ushort ReservedSectorsCount { get; set; } = 32;
        public byte NumberOfFATs { get; set; } = 2;
        public byte MediaType { get; set; } = 0xF8;
        public ushort SectorsPerTrack { get; set; }
        public ushort NumberOfHeads { get; set; }
        public uint HiddenSectors { get; set; }
        public uint TotalSectors { get; set; }
        public uint FATsize { get; set; }
        public ushort ExtensionFlags { get; set; } = 0;
        public ushort FileSystemVersion { get; set; } = 0;
        public uint RootClusterNumber { get; set; } = 2;
        public ushort FSInfoSectorNumber { get; set; } = 1;
        public ushort BackupSectorNumber { get; set; } = 6;
        public byte DriveNumber { get; set; } = 0x80;
        public uint VolumeID { get; set; }
        public string VolumeLabel { get; set; } = "NO NAME    ";


        public BootSector(int sectorSize)
        {
            base.Data = new Buffer(sectorSize);
            base.Size = sectorSize;
        }

        public override void Build()
        {
            base.Data.Add(JMPBoot, 0, 3);
            base.Data.Add(OEMName, 3);
            base.Data.Add(BytesPerSector, 11);
            base.Data.Add(SectorsPerCluster, 13);
            base.Data.Add(ReservedSectorsCount, 14);
            base.Data.Add(NumberOfFATs, 16);
            base.Data.Add(RootEntriesCount, 17);
            base.Data.Add(TotalSectors16, 19);
            base.Data.Add(MediaType, 21);
            base.Data.Add(FAT16sizeInSectors, 22);
            base.Data.Add(SectorsPerTrack, 24);
            base.Data.Add(NumberOfHeads, 26);
            base.Data.Add(HiddenSectors, 28);
            base.Data.Add(TotalSectors, 32);
            base.Data.Add(FATsize, 36);
            base.Data.Add(ExtensionFlags, 40);
            base.Data.Add(FileSystemVersion, 42);
            base.Data.Add(RootClusterNumber, 44);
            base.Data.Add(FSInfoSectorNumber, 48);
            base.Data.Add(BackupSectorNumber, 50);
            base.Data.Add(DriveNumber, 64);
            base.Data.Add(BootSignature, 66);
            base.Data.Add(VolumeID, 67);
            base.Data.Add(VolumeLabel, 71);
            base.Data.Add(FileSystemType, 82);

            base.Data.Add(Checksum, 510, 2);

            // Not required, but recommended
            if (base.Size > 512)
                base.Data.Add(Checksum, (int)base.Size - 2, 2);
        }
    }
}