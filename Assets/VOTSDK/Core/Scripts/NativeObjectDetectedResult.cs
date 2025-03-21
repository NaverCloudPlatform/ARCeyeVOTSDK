using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ARCeye.VOT
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeDetectedObject
    {
        public string name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 27)]
        public float[] corners3d;
        public float probability;
        public int status;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] modelMatrix;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct NativeObjectDetectedResult
    {
        [MarshalAs(UnmanagedType.I4)]
        public int objectsSize;

        public IntPtr objects;
    }
}