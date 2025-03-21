using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ARCeye.VOT
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct NativeVOTFrame
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string uuid;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] viewMatrix;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] texMatrix;

        public float fx;
        public float fy;
        public float cx;
        public float cy;

        public IntPtr imageBuffer;
    }
}