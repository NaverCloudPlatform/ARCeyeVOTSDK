using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ARCeye.VOT
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct NativeVOTURL
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string invokeUrl;

        [MarshalAs(UnmanagedType.LPStr)]
        public string secretKey;
    }
}