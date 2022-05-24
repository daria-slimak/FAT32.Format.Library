using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FAT32.Format.Library
{
    internal static class Win32
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
            string lpFileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);
    }
}
