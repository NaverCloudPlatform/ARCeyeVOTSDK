using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ARCeye.VOT
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class RequestVOTInfo
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string uuid;
        [MarshalAs(UnmanagedType.LPStr)]
        public string method;
        [MarshalAs(UnmanagedType.LPStr)]
        public string invokeUrl;
        [MarshalAs(UnmanagedType.LPStr)]
        public string secretKey;
        [MarshalAs(UnmanagedType.LPStr)]
        public string timestamp;
        [MarshalAs(UnmanagedType.LPStr)]
        public string intrinsic;
        [MarshalAs(UnmanagedType.LPStr)]
        public string extrinsic;
        [MarshalAs(UnmanagedType.LPStr)]
        public string distortion;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] texMatrix;
        public IntPtr rawImage;
    }
}