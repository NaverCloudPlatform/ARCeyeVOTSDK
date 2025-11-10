using System.Collections;
using System.Collections.Generic;
using System.Text;
using ARCeye.Dataset;
using UnityEngine;
using UnityEngine.UI;

namespace ARCeye.VOT.Samples.ARDataset
{
    public class VOTSDKARDatasetExample : MonoBehaviour
    {
        public VOTSDKManager m_VOTSDKManager;
        public ARDatasetManager m_ARDatasetManager;
        public GameObject m_ResultCubePrefab;

        private ResultCube m_ResultCube = null;
        public RawImage m_RequestImage;

        private Camera m_MainCamera;

        private bool m_IsVOTRunning = false;

        public GameObject m_LogPanel;
        public Text m_RequestText;
        public Text m_ResponseText;


        private void Start()
        {
            m_LogPanel.SetActive(false);

            m_MainCamera = Camera.main;

            m_ARDatasetManager.frameReceived += OnFrameReceived;
            m_ARDatasetManager.Play();
        }

        private void OnFrameReceived(FrameData frameData)
        {
            if (!m_IsVOTRunning)
            {
                return;
            }

            // Update camera pose.
            m_MainCamera.transform.position = frameData.modelMatrix.GetColumn(3);
            m_MainCamera.transform.rotation = Quaternion.LookRotation(frameData.modelMatrix.GetColumn(2), frameData.modelMatrix.GetColumn(1));
            m_MainCamera.fieldOfView = CalculateFOV(frameData.projMatrix);

            // Update preview.
            m_ARDatasetManager.TryAcquireFrameImage(out Texture frameTexture);
            m_ARDatasetManager.SetPreviewTexture(frameTexture);

            // Request VOT.
            VOTFrame votFrame = VOTFrame.Builder()
                .SetCameraParameters(frameData.intrinsic.fx, frameData.intrinsic.fy, frameData.intrinsic.cx, frameData.intrinsic.cy)
                .SetImageBuffer(frameTexture)
                .Build();
            m_VOTSDKManager.Request(votFrame);
        }

        private float CalculateFOV(Matrix4x4 projMatrix)
        {
            float f = projMatrix.m11;
            float verticalFOV = 2.0f * Mathf.Atan(1.0f / f) * Mathf.Rad2Deg;
            return verticalFOV;
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