using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace ARCeye.VOT
{
    public class NativeBridge
    {
        //
        //  Native Bridge
        //

#if UNITY_IOS && !UNITY_EDITOR
        public const string dll = "__Internal";
#else
        public const string dll = "VOTSDK";
#endif

        public delegate void OnObjectDetectedDelegate(IntPtr objectDetectedResultPtr);
        public delegate void OnObjectRemovedDelegate(string removedObjectName);


        [DllImport(dll)]
        public static extern IntPtr GetVersionNative();

        [DllImport(dll)]
        public static extern void InitVOTSDKNative(ref NativeConfig config);

        [DllImport(dll)]
        public static extern void DestroyPluginNative();

        [DllImport(dll)]
        public static extern bool IsInitializedNative();

        [DllImport(dll)]
        public static extern bool ResetNative();

        [DllImport(dll)]
        public static extern bool SetOnObjectDetectedCallbackNative(OnObjectDetectedDelegate func);

        [DllImport(dll)]
        public static extern bool SetOnObjectRemovedCallbackNative(OnObjectRemovedDelegate func);


        [DllImport(dll)]
        public static extern bool RequestNative(ref NativeVOTFrame votFrame);


        //
        //  Network
        //

        [DllImport(dll)]
        public static extern void SendResponseToNative(IntPtr rawPtr);


        //
        //  Logger
        //

        public delegate void DebugLogFuncDelegate(LogLevel level, IntPtr message);

        [DllImport(dll)]
        public static extern void VOTSDK_SetDebugLogFuncNative(DebugLogFuncDelegate func);
        [DllImport(dll)]
        public static extern void VOTSDK_ReleaseLoggerNative();



        //
        //  Converter
        //

        public static NativeConfig ConvertToNative(Config config)
        {
            NativeConfig nativeConfig = new NativeConfig();

            // VOTURL 배열을 NativeVOTURL으로 변환.
            VOTURL[] urlList = config.urlList;
            NativeVOTURL[] nativeVOTURLs = new NativeVOTURL[urlList.Length];

            for (int i = 0; i < urlList.Length; i++)
            {
                VOTURL url = urlList[i];
                NativeVOTURL nativeUrl = NativeBridge.ConvertToNative(url);
                nativeVOTURLs[i] = nativeUrl;
            }

            int nativeVOTURLsSize = Marshal.SizeOf(typeof(NativeVOTURL)) * nativeVOTURLs.Length;
            IntPtr nativeVOTURLsPtr = Marshal.AllocHGlobal(nativeVOTURLsSize);

            for (int i = 0; i < nativeVOTURLs.Length; i++)
            {
                // native 영역에 생성한 nativeVOTURLsPtr 구조체 배열의 시작 위치를 이용하여 IntPtr 생성.
                IntPtr nativeVOTURLPtr = new IntPtr(nativeVOTURLsPtr.ToInt64() + i * Marshal.SizeOf(typeof(NativeVOTURL)));
                Marshal.StructureToPtr(nativeVOTURLs[i], nativeVOTURLPtr, false);
            }

            nativeConfig.urlList = nativeVOTURLsPtr;
            nativeConfig.urlListLength = nativeVOTURLs.Length;
            nativeConfig.requestInterval = config.requestInterval;
            nativeConfig.poseFilterCapacity = config.poseFilterCapacity;
            nativeConfig.logLevel = config.logLevel;

            return nativeConfig;
        }

        public static NativeVOTURL ConvertToNative(VOTURL votURL)
        {
            NativeVOTURL nativeVOTURL = new NativeVOTURL();

            nativeVOTURL.invokeUrl = votURL.invokeUrl;
            nativeVOTURL.secretKey = votURL.secretKey;

            return nativeVOTURL;
        }

        public static ObjectDetectedResult ConvertToObjectDetectedResultFromRawPtr(IntPtr resultPtr)
        {
            NativeObjectDetectedResult nativeResult = Marshal.PtrToStructure<NativeObjectDetectedResult>(resultPtr);

            ObjectDetectedResult result = new ObjectDetectedResult();

            for (int i = 0; i < nativeResult.objectsSize; i++)
            {
                IntPtr objectPtr = new IntPtr(nativeResult.objects.ToInt64() + i * Marshal.SizeOf(typeof(NativeDetectedObject)));
                NativeDetectedObject nativeDetectedObject = Marshal.PtrToStructure<NativeDetectedObject>(objectPtr);

                DetectedObjectData detectedObject = NativeBridge.ConvertFromNative(nativeDetectedObject);
                result.detectedObjects.Add(detectedObject);
            }

            return result;
        }

        public static DetectedObjectData ConvertFromNative(NativeDetectedObject nativeDetectedObject)
        {
            DetectedObjectData detectedObject = new DetectedObjectData();

            detectedObject.name = nativeDetectedObject.name;
            detectedObject.probability = nativeDetectedObject.probability;
            float[] modelMatrix = nativeDetectedObject.modelMatrix;

            // Native에서 extrinsic과 EC objectPose를 이용해서 WC 기준의 object model matrix를 계산.
            Matrix4x4 objectModelMatrixCV = PoseUtility.FloarArrayToMatrix4x4(modelMatrix);
            Matrix4x4 objectModelMatrixUnity = PoseUtility.UnityToCVMatrix4x4(objectModelMatrixCV);
            // 오브젝트 태깅은 OpenCV 좌표계에서 진행. 카메라의 Up과 오브젝의 Up이 반대.
            // Unity는 카메라의 up과 오브젝트의 up이 같은 방향.
            // Native에서 계산된 object model matrix를 그대로 사용하면 상하가 뒤집어진 결과가 나오게 됨.
            // VOT를 통해 생성된 오브젝트를 Unity에 올려서 좌표계를 매칭해보면
            // z축으로 180도 회전을 시키면 동일한 결과를 얻을 수 있다는 것을 알 수 있다.
            Matrix4x4 objectModelMatrixFlipY = PoseUtility.CVToUnityLocal(objectModelMatrixUnity);

            Vector3 position = PoseUtility.ExtractPosition(objectModelMatrixFlipY);
            Quaternion rotation = PoseUtility.ExtractRotation(objectModelMatrixFlipY);

            detectedObject.position = position;
            detectedObject.rotation = rotation;


            // 물체 사이즈 설정.
            // nativeDetectedObject.corners3d[0]은 항상 원점
            float width = Mathf.Abs(nativeDetectedObject.corners3d[3] * 2);
            float height = Mathf.Abs(nativeDetectedObject.corners3d[4] * 2);
            float depth = Mathf.Abs(nativeDetectedObject.corners3d[5] * 2);
            detectedObject.size = new Vector3(width, height, depth);

            // Corner3D 좌표계 변경.
            List<Vector3> corner3d = new List<Vector3>();
            for (int i = 0; i < nativeDetectedObject.corners3d.Length / 3; i++)
            {
                float nCornerX = nativeDetectedObject.corners3d[i * 3 + 0];
                float nCornerY = nativeDetectedObject.corners3d[i * 3 + 1];
                float nCornerZ = nativeDetectedObject.corners3d[i * 3 + 2];
                Vector3 nativeCorner = new Vector3(nCornerX, nCornerY, nCornerZ);
                Vector3 corner = PoseUtility.ConvertCVToUnity(nativeCorner);

                Vector3 wcCorner = objectModelMatrixFlipY.MultiplyPoint(corner);

                corner3d.Add(wcCorner);
            }

            detectedObject.corners3d = corner3d;

            return detectedObject;
        }


        //
        //  VOTFrame
        //

        public static NativeVOTFrame ConvertToNative(VOTFrame votFrame)
        {
            NativeVOTFrame nativeVOTFrame = new NativeVOTFrame();

            nativeVOTFrame.uuid = votFrame.uuid;

            Matrix4x4 lhCamModelMatrix = Matrix4x4.TRS(votFrame.position, votFrame.rotation, Vector3.one);
            Matrix4x4 cvModelMatrix = PoseUtility.CVToUnityMatrix4x4(lhCamModelMatrix);
            Matrix4x4 viewMatrix = Matrix4x4.Inverse(cvModelMatrix);

            // row major matrix로 전달.
            nativeVOTFrame.viewMatrix = viewMatrix.ToData();
            nativeVOTFrame.texMatrix = votFrame.texMatrix.ToData();

            nativeVOTFrame.fx = votFrame.fx;
            nativeVOTFrame.fy = votFrame.fy;
            nativeVOTFrame.cx = votFrame.cx;
            nativeVOTFrame.cy = votFrame.cy;

            GCHandle gcI = GCHandle.Alloc(votFrame.imageTexture, GCHandleType.Weak);
            nativeVOTFrame.imageBuffer = GCHandle.ToIntPtr(gcI);

            return nativeVOTFrame;
        }
    }


    internal static class Extension
    {
        public static float[] ToData(this Matrix4x4 m)
        {
            float[] res = {
                m[0, 0], m[0, 1], m[0, 2], m[0, 3],
                m[1, 0], m[1, 1], m[1, 2], m[1, 3],
                m[2, 0], m[2, 1], m[2, 2], m[2, 3],
                m[3, 0], m[3, 1], m[3, 2], m[3, 3],
            };
            return res;
        }
    }
}