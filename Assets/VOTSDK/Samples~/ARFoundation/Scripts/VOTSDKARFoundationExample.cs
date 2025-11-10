using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARCeye.VOT.Samples.ARFoundation
{
    public class VOTSDKARFoundationExample : MonoBehaviour
    {
        public VOTSDKManager m_VOTSDKManager;
        public ARCameraManager m_ARCameraManager;
        public GameObject m_ResultCubePrefab;

        private Texture2D m_CameraRequestTexture = null;
        private ResultCube m_ResultCube = null;

        public RawImage m_RequestImage;

        private bool m_IsVOTRunning = false;

        public GameObject m_LogPanel;
        public Text m_RequestText;
        public Text m_ResponseText;

        private void Start()
        {
            m_LogPanel.SetActive(false);

            m_ARCameraManager.frameReceived += OnFrameReceived;
        }

        private void OnFrameReceived(ARCameraFrameEventArgs eventArgs)
        {
            if (!m_IsVOTRunning)
            {
                return;
            }

            // Fetch camera frame data.
            if (!m_ARCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            {
                Debug.LogError("Failed to acquire camera image.");
                return;
            }
            if (!m_ARCameraManager.TryGetIntrinsics(out XRCameraIntrinsics cameraIntrinsics))
            {
                Debug.LogError("Failed to acquire camera intrinsics.");
                return;
            }

            // Scale down the image to 640x480 for better performance.
            const int MAJOR_AXIS_LENGTH = 640;
            float scale = (float)MAJOR_AXIS_LENGTH / (float)image.width;

            // Set camera parameters.
            // transpose focal length and principal point in portrait mode.
            float fx = cameraIntrinsics.focalLength.y * scale;
            float fy = cameraIntrinsics.focalLength.x * scale;
            float cx = cameraIntrinsics.principalPoint.y * scale;
            float cy = cameraIntrinsics.principalPoint.x * scale;

            // Request VOT.
            VOTFrame votFrame = VOTFrame.Builder()
                .SetCameraParameters(fx, fy, cx, cy)
                .SetImageBuffer(GetPreviewTexture(image, scale), eventArgs.displayMatrix.Value)
                .Build();
            m_VOTSDKManager.Request(votFrame);

            image.Dispose();
        }

        unsafe private Texture2D GetPreviewTexture(XRCpuImage image, float scale)
        {
            int scaledWidth = Mathf.FloorToInt(image.width * scale);
            int scaledHeight = Mathf.FloorToInt(image.height * scale);

            if (m_CameraRequestTexture == null || m_CameraRequestTexture.width != scaledWidth || m_CameraRequestTexture.height != scaledHeight)
            {
                m_CameraRequestTexture = new Texture2D(scaledWidth, scaledHeight, TextureFormat.BGRA32, false);
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            XRCpuImage.Transformation transformation = XRCpuImage.Transformation.MirrorX;
#else
            XRCpuImage.Transformation transformation = XRCpuImage.Transformation.None;
#endif

            var buffer = m_CameraRequestTexture.GetRawTextureData<byte>();

            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, image.width, image.height),
                outputDimensions = new Vector2Int(scaledWidth, scaledHeight),
                outputFormat = TextureFormat.BGRA32,
                transformation = transformation
            };

            image.Convert(conversionParams, new System.IntPtr(buffer.GetUnsafePtr()), buffer.Length);

            m_CameraRequestTexture.Apply();

            return m_CameraRequestTexture;
        }

        public void OnVOTRequested(VOTRequestEventData eventData)
        {
            Debug.Log("Request VOT : " + eventData.RequestBody);

            float width = eventData.RequestTexture.width;
            float height = eventData.RequestTexture.height;

            m_RequestImage.texture = eventData.RequestTexture;
            m_RequestImage.rectTransform.sizeDelta = new Vector2(width, height);

            m_RequestText.text = eventData.RequestBody;
        }

        public void OnVOTResponded(VOTResponseEventData eventData)
        {
            Debug.Log("Response VOT : " + eventData.ResponseBody);

            m_ResponseText.text = eventData.ResponseBody;
        }

        public void OnObjectDetected(ObjectDetectedResult result)
        {
            if (result.detectedObjects.Count > 0)
            {
                if (m_ResultCube == null)
                {
                    GameObject go = Instantiate(m_ResultCubePrefab);
                    m_ResultCube = go.GetComponent<ResultCube>();
                }

                Debug.Log("Detected object : " + result.detectedObjects[0].ToString());

                DetectedObjectData detectedObject = result.detectedObjects[0];
                m_ResultCube.Initialize(detectedObject);
            }
        }

        public void StartVOT()
        {
            m_IsVOTRunning = true;
        }

        public void StopVOT()
        {
            m_IsVOTRunning = false;
        }

        public void ResetVOT()
        {
            m_VOTSDKManager.ResetSession();

            if (m_ResultCube != null)
            {
                Destroy(m_ResultCube.gameObject);
                m_ResultCube = null;
            }
        }

        public void ToggleLog()
        {
            m_LogPanel.SetActive(!m_LogPanel.activeSelf);
        }
    }
}