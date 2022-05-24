namespace FAT32.Format.Library
{
    internal abstract class Sector
    {
        public Buffer Data { get; protected set; }
        public int Size { get; protected set; }

        public abstract void Build();
    }
}