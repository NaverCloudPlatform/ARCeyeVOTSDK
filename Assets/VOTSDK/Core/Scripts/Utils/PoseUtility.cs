using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ARCeye.VOT
{
    public class PoseUtility
    {
        static private Matrix4x4 m_FilpX = new Matrix4x4(
            new Vector4(-1, 0, 0, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(0, 0, 0, 1)
        );
        static private Matrix4x4 m_FilpZ = new Matrix4x4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, -1, 0),
            new Vector4(0, 0, 0, 1)
        );
        static private Matrix4x4 m_CVGL = new Matrix4x4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, -1, 0, 0),
            new Vector4(0, 0, -1, 0),
            new Vector4(0, 0, 0, 1)
        );
        static private Matrix4x4 m_Zm180Local = new Matrix4x4(
            new Vector4(-1, 0, 0, 0),
            new Vector4(0, -1, 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(0, 0, 0, 1)
        );

        static public Matrix4x4 UnmanagedToMatrix4x4<T>(IntPtr ptr)
        {
            if (typeof(T) != typeof(float) && typeof(T) != typeof(double))
            {
                throw new ArgumentException("T must be either int, float, or double");
            }

            if (typeof(T) != typeof(float))
            {
                float[] m = new float[16];
                Marshal.Copy(ptr, m, 0, 16);
                return new Matrix4x4(
                    new Vector4(m[0], m[1], m[2], m[3]),
                    new Vector4(m[4], m[5], m[6], m[7]),
                    new Vector4(m[8], m[9], m[10], m[11]),
                    new Vector4(m[12], m[13], m[14], m[15])
                );
            }
            else
            {
                double[] m = new double[16];
                Marshal.Copy(ptr, m, 0, 16);
                return new Matrix4x4(
                    new Vector4((float)m[0], (float)m[1], (float)m[2], (float)m[3]),
                    new Vector4((float)m[4], (float)m[5], (float)m[6], (float)m[7]),
                    new Vector4((float)m[8], (float)m[9], (float)m[10], (float)m[11]),
                    new Vector4((float)m[12], (float)m[13], (float)m[14], (float)m[15])
                );
            }
        }

        static public Matrix4x4 UnmanagedToMatrix4x4From3x3(IntPtr ptr)
        {
            float[] m = new float[9];
            Marshal.Copy(ptr, m, 0, 9);
            return new Matrix4x4(
                new Vector4(m[0], m[1], m[2], 0),
                new Vector4(m[3], m[4], m[5], 0),
                new Vector4(m[6], m[7], m[8], 0),
                new Vector4(0, 0, 0, 1)
            );
        }

        static public Matrix4x4 FloarArrayToMatrix4x4(float[] arr)
        {
            return new Matrix4x4(
                new Vector4(arr[0], arr[4], arr[8], arr[12]),
                new Vector4(arr[1], arr[5], arr[9], arr[13]),
                new Vector4(arr[2], arr[6], arr[10], arr[14]),
                new Vector4(arr[3], arr[7], arr[11], arr[15])
            );
        }

        static public byte[] UnmanagedToByteArray(IntPtr ptr, int length)
        {
            byte[] bytes = new byte[length];
            Marshal.Copy(ptr, bytes, 0, length);
            return bytes;
        }

        static public Matrix4x4 FlipZ(Matrix4x4 src)
        {
            return m_FilpZ * src;
        }

        static public Matrix4x4 ConvertLHRH(Matrix4x4 src)
        {
            return m_FilpX * src * m_FilpZ;
        }

        static public Matrix4x4 ConvertLHRHView(Matrix4x4 src)
        {
            return m_FilpZ * src * m_FilpX;
        }

        static public Matrix4x4 ConvertCVtoGL(Matrix4x4 m)
        {
            Matrix4x4 dst = m_CVGL * m;
            return dst;
        }

        static public Matrix4x4 ConvertGLtoCV(Matrix4x4 m)
        {
            Matrix4x4 dst = m_CVGL * m;
            return dst;
        }

        static public Vector3 ConvertCVToUnity(Vector3 cvPoint)
        {
            Vector3 glPoint = m_CVGL.MultiplyPoint(cvPoint);
            return m_FilpZ.MultiplyPoint(glPoint);
        }

        static public bool IsValidScale(Matrix4x4 m)
        {
            var scale = m.lossyScale;
            return scale.x > 0 && scale.y > 0 && scale.z > 0;
        }

        public static Vector3 ExtractPosition(Matrix4x4 matrix)
        {
            return new Vector3(matrix.m03, matrix.m13, matrix.m23);
        }

        public static Quaternion ExtractRotation(Matrix4x4 matrix)
        {
            // Extract the rotation as a Quaternion from the upper-left 3x3 portion of the matrix
            return Quaternion.LookRotation(
                new Vector3(matrix.m02, matrix.m12, matrix.m22), // Forward vector
                new Vector3(matrix.m01, matrix.m11, matrix.m21)  // Up vector
            );
        }


        public static Matrix4x4 UnityToCVMatrix4x4(Matrix4x4 unityMatrix)
        {
            Matrix4x4 flipY = Matrix4x4.Scale(new Vector3(1, -1, 1));
            return flipY * unityMatrix * flipY.inverse;
        }

        public static Matrix4x4 CVToUnityMatrix4x4(Matrix4x4 unityMatrix)
        {
            Matrix4x4 flipY = Matrix4x4.Scale(new Vector3(1, -1, 1));
            return flipY * unityMatrix * flipY.inverse;
        }

        public static Matrix4x4 CVToUnityLocal(Matrix4x4 modelMatrix)
        {
            return modelMatrix * m_Zm180Local;
        }
    }
}