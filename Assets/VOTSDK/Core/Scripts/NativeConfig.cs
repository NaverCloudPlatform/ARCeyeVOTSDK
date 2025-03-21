using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ARCeye.VOT
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct NativeConfig
    {
        public IntPtr urlList;

        [MarshalAs(UnmanagedType.I8)]
        public long urlListLength;

        [MarshalAs(UnmanagedType.I8)]
        public long requestInterval;

        [MarshalAs(UnmanagedType.I8)]
        public long poseFilterCapacity;

        [MarshalAs(UnmanagedType.I4)]
        public LogLevel logLevel;
    }
}