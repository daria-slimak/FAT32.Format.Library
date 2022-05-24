using DeviceIOControlLib.Objects.Disk;
using DeviceIOControlLib.Wrapper;
using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace FAT32.Format.Library
{
    internal class Disk : IDisposable
    {
        private SafeFileHandle _handle;
        private FileStream _stream;
        private DiskDeviceWrapper _device;
        private DISK_GEOMETRY _diskGeometry;
        private PARTITION_INFORMATION _partitionInformation;
        private PARTITION_INFORMATION_EX _partitionInformationEx;

        public long TotalSize { get; private set; }
        public int BytesPerSector { get; private set; }
        public long TotalSectors { get; private set; }
        public short SectorsPerTrack { get; private set; }
        public short NumberOfHeads { get; private set; }
        public int HiddenSectors { get; private set; }

        public Disk(char letter)
        {
            string dosName = $@"\\.\{letter}:".ToUpper();

            _handle = Win32.CreateFile(dosName, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);

            if (_handle.IsInvalid)
            {
                int error = Marshal.GetLastWin32Error();

                throw new ArgumentException($"Invalid drive letter '{letter}:'\nError {error}: {new Win32Exception(error).Message}");
            }

            Lock();
            Unmount();

            _stream = new FileStream(_handle, FileAccess.ReadWrite);
            _device = new DiskDeviceWrapper(_handle);

            _diskGeometry = _device.DiskGetDriveGeometry();

            BytesPerSector = _diskGeometry.BytesPerSector;
            SectorsPerTrack = (short)_diskGeometry.SectorsPerTrack;
            NumberOfHeads = (short)_diskGeometry.TracksPerCylinder;

            try
            {
                _partitionInformation = _device.DiskGetPartitionInfo();

                TotalSize = _partitionInformation.PartitionLength;
                TotalSectors = _partitionInformation.PartitionLength / BytesPerSector;
                HiddenSectors = _partitionInformation.HiddenSectors;
            }

            catch // If GPT, then DiskGetPartitionInfo throw exception
            {
                try
                {
                    _partitionInformationEx = _device.DiskGetPartitionInfoEx();

                    TotalSize = _partitionInformationEx.PartitionLength;
                    TotalSectors = _partitionInformationEx.PartitionLength / BytesPerSector;
                    HiddenSectors = (int)_partitionInformationEx.StartingOffset / BytesPerSector;
                }

                catch
                {
                    throw new DiskException("Unable to retrieve partition information");
                }
            }

            if (TotalSize == 0)
                throw new DiskException("Drive size is 0 bytes");

            if (TotalSize >= 0x20000000000)
                throw new DiskException("Drive size is too big. Max. is 2TB");

            if (TotalSize <= 0x2000A00)
                throw new DiskException("Drive size is too small. Min. is 32MB");
        }

        public void Write(byte[] buffer, long offset, int count)
        {
            try
            {
                _stream.Seek(offset, SeekOrigin.Begin);
                _stream.Write(buffer, 0, count);
            }

            catch
            {
                throw new IOException("Unable to write");
            }
        }

        public void WriteSector(int sector, Buffer buffer, int count)
        {
            Write(buffer.Get(), sector * BytesPerSector, count);
        }

        public void Lock()
        {
            if (!DeviceIoControlHelper.InvokeIoControl(_handle, DeviceIOControlLib.Objects.Enums.IOControlCode.FsctlLockVolume))
                throw new IOException("Unable to lock disk");
        }

        public void Unlock()
        {
            if (!DeviceIoControlHelper.InvokeIoControl(_handle, DeviceIOControlLib.Objects.Enums.IOControlCode.FsctlUnlockVolume))
                throw new IOException("Unable to unlock disk");
        }

        public void Unmount()
        {
            if (!DeviceIoControlHelper.InvokeIoControl(_handle, DeviceIOControlLib.Objects.Enums.IOControlCode.FsctlDismountVolume))
                throw new IOException("Unable to unmount disk");
        }

        public void Dispose()
        {
            Unlock();

            if (!_handle.IsClosed)
                _handle.Dispose();
        }
    }
}