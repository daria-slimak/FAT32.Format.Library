namespace FAT32.Format.Library
{
    internal class FSInfoSector : Sector
    {
        /*
         * fatgen103.doc   Size : Offset
         * 
         * FSI_LeadSig        4 : 0 byte
         * FSI_Reserved1    480 : 4 byte
         * FSI_StrucSig       4 : 484 byte
         * FSI_FreeCount      4 : 488 byte
         * FSI_NxtFree        4 : 492 byte
         * FSI_Reserved2     12 : 496 byte
         * FSI_TrailSig       4 : 508 byte
         * 
         */

        private readonly uint LeadSignature = 0x41615252;
        private readonly uint StrucSignature = 0x61417272;
        private readonly uint TrailSignature = 0xAA550000;

        public uint FreeClusters { get; set; } = 0xFFFFFFFF;
        public uint FirstFreeCluster { get; set; } = 3;      

        public FSInfoSector(int sectorSize)
        {
            base.Data = new Buffer(sectorSize);
            base.Size = sectorSize;
        }

        public override void Build()
        {
            base.Data.Add(LeadSignature, 0);
            base.Data.Add(StrucSignature, 484);
            base.Data.Add(FreeClusters, 488);
            base.Data.Add(FirstFreeCluster, 492);
            base.Data.Add(TrailSignature, 508);
        }
    }
}