using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using UnityEngine.Events;

namespace ARCeye.VOT
{
    public class VOTSDKManager : MonoBehaviour
    {
        private const string PACKAGE_VERSION = "0.9.0";

        private static VOTSDKManager s_Instance;
        private Transform m_ARCamera;

        public bool IsInitialized => NativeBridge.IsInitializedNative();

        private NetworkController m_NetworkController;
        public NetworkController networkController
        {
            get => m_NetworkController;
            set => m_NetworkController = value;
        }


        [SerializeField]
        private VOTSDKSettings m_Settings;
        public VOTSDKSettings settings
        {
            get => m_Settings;
            set => m_Settings = value;
        }

        [Header("Event")]
        [SerializeField, Tooltip("Event triggered when request VOT.")]
        private UnityEvent<VOTRequestEventData> m_OnVOTRequested = new UnityEvent<VOTRequestEventData>();
        public UnityEvent<VOTRequestEventData> OnVOTRequested
        {
            get => m_OnVOTRequested;
            set
            {
                m_NetworkController.OnVOTRequested = value;
                m_OnVOTRequested = value;
            }
        }

        [SerializeField, Tooltip("Event triggered when VOT response is received.")]
        private UnityEvent<VOTResponseEventData> m_OnVOTResponded = new UnityEvent<VOTResponseEventData>();
        public UnityEvent<VOTResponseEventData> OnVOTResponded
        {
            get => m_OnVOTResponded;
            set
            {
                m_NetworkController.OnVOTResponded = value;
                m_OnVOTResponded = value;
            }
        }

        [SerializeField, Tooltip("Event triggered when an object is detected.")]
        private UnityEvent<ObjectDetectedResult> m_OnObjectDetected = new UnityEvent<ObjectDetectedResult>();
        public UnityEvent<ObjectDetectedResult> OnObjectDetected => m_OnObjectDetected;


        [SerializeField, Tooltip("Event triggered when an object is removed.")]
        private UnityEvent<string> m_OnObjectRemoved = new UnityEvent<string>();
        public UnityEvent<string> OnObjectRemoved => m_OnObjectRemoved;


        private void Awake()
        {
            s_Instance = this;

            if (m_Settings != null)
            {
                Initialize();
            }
            else
            {
                Debug.Log("[VOTManager] VOTSDKSettings isn't assigned. Initialize() method should be called after assigning VOTSDKSettings");
            }
        }


        private void OnDestroy()
        {
            NativeBridge.DestroyPluginNative();
        }

        public void Initialize()
        {
            if (!IsInitialized)
            {
                InitLogLevel();
                InitARCamera();
                InitNetworkController();

                if (m_Settings == null)
                {
                    Debug.LogError("VOTSDKSettings is not assigned to VOTManager");
                    return;
                }

                // InitPoseTracker();

                Config config = new Config(m_Settings);
                NativeConfig nativeConfig = NativeBridge.ConvertToNative(config);
                NativeBridge.InitVOTSDKNative(ref nativeConfig);
                NativeBridge.SetOnObjectDetectedCallbackNative(OnObjectDetectedCallbackNative);
                NativeBridge.SetOnObjectRemovedCallbackNative(OnObjectRemovedCallbackNative);
            }
        }

        private void InitLogLevel()
        {
            LogViewer logViewer = GetComponent<LogViewer>();
            if (logViewer)
            {
                logViewer.logLevel = m_Settings.logLevel;
            }
            else
            {
                Debug.LogWarning("Failed to find LogViewer");
            }
        }

        private void InitARCamera()
        {
            if (Camera.main)
            {
                m_ARCamera = Camera.main.transform;
            }
        }

        private void InitNetworkController()
        {
            if (m_NetworkController == null)
            {
                m_NetworkController = GetComponentInChildren<UnityNetworkController>();
                if (m_NetworkController == null)
                {
                    Debug.LogWarning("[VOTManager] Failed to find UnityNetworkController under VOTManager");
                }
            }

            m_NetworkController.OnVOTRequested = m_OnVOTRequested;
            m_NetworkController.OnVOTResponded = m_OnVOTResponded;
        }

        public void ResetSession()
        {
            if (!IsInitialized)
            {
                Debug.LogError("[VOTManager] VOTManager isn't initialized!");
                return;
            }

            NativeBridge.ResetNative();
        }

        public void Request(VOTFrame votFrame)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[VOTManager] VOTManager isn't initialized!");
                return;
            }

            NativeVOTFrame nativeVOTFrame = NativeBridge.ConvertToNative(votFrame);
            NativeBridge.RequestNative(ref nativeVOTFrame);
        }

        public string GetPackageVersion()
        {
            return PACKAGE_VERSION;
        }

        public string GetPluginVersion()
        {
            IntPtr strPtr = NativeBridge.GetVersionNative();
            return Marshal.PtrToStringAnsi(strPtr);
        }


        [MonoPInvokeCallback(typeof(NativeBridge.OnObjectDetectedDelegate))]
        static public void OnObjectDetectedCallbackNative(IntPtr objectDetectedResultPtr)
        {
            ObjectDetectedResult nativeResult = NativeBridge.ConvertToObjectDetectedResultFromRawPtr(objectDetectedResultPtr);

            s_Instance.OnObjectDetected?.Invoke(nativeResult);
        }

        [MonoPInvokeCallback(typeof(NativeBridge.OnObjectRemovedDelegate))]
        static public void OnObjectRemovedCallbackNative(string removedObjectName)
        {
            s_Instance.OnObjectRemoved?.Invoke(removedObjectName);
        }
    }
}
