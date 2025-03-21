using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ARCeye.VOT
{
    public class VOTSimpleRequest : MonoBehaviour
    {
        [SerializeField] private VOTSDKManager m_VOTSDKManager;
        [SerializeField] private Texture2D[] m_RequestTextures;
        [SerializeField] private RawImage m_RequestImage;
        [SerializeField] private Text m_RequestResult;
        [SerializeField] private GameObject m_ResultCubePrefab;

        private ResultCube m_ResultCube = null;

        private int m_TextureIndex = 0;

        private const int k_Intrinsic_Fx = 447;
        private const int k_Intrinsic_Fy = 447;
        private const int k_Intrinsic_Cx = 180;
        private const int k_Intrinsic_Cy = 322;


        private void Start()
        {
            Debug.Log("VOTSDK package version : " + m_VOTSDKManager.GetPackageVersion());
            Debug.Log("VOTSDK native version : " + m_VOTSDKManager.GetPluginVersion());
        }

        public void ResetVOT()
        {
            m_VOTSDKManager.ResetSession();
        }

        public void RequestVOT()
        {
            Texture requestTexture = m_RequestTextures[m_TextureIndex++ % m_RequestTextures.Length];

            VOTFrame votFrame = VOTFrame.Builder()
                .SetCameraParameters(k_Intrinsic_Fx, k_Intrinsic_Fy, k_Intrinsic_Cx, k_Intrinsic_Cy)
                .SetImageBuffer(requestTexture)
                .Build();
            m_VOTSDKManager.Request(votFrame);
        }

        public void OnVOTRequested(VOTRequestEventData eventData)
        {
            Debug.Log("Request VOT : " + eventData.Url);
            Debug.Log("Request Body: " + eventData.RequestBody);

            m_RequestImage.texture = eventData.RequestTexture;
            m_RequestImage.color = Color.white;
        }

        public void OnVOTResponded(VOTResponseEventData eventData)
        {
            Debug.Log("Response VOT : " + eventData.ResponseBody);
        }

        public void OnObjectDetected(ObjectDetectedResult result)
        {
            // 전체 응답 결과 출력.
            StringBuilder sb = new StringBuilder();

            sb.Append("Detected Objects \n");

            for (int i = 0; i < result.detectedObjects.Count; i++)
            {
                DetectedObjectData detectedObject = result.detectedObjects[i];
                sb.Append(detectedObject);
            }

            m_RequestResult.text = sb.ToString();


            // 첫 번째 응답에 대한 결과 시각화.
            if (result.detectedObjects.Count > 0)
            {
                if (m_ResultCube == null)
                {
                    GameObject go = Instantiate(m_ResultCubePrefab);
                    m_ResultCube = go.GetComponent<ResultCube>();
                }

                DetectedObjectData detectedObject = result.detectedObjects[0];
                m_ResultCube.Initialize(detectedObject);
            }
        }

        public void OnObjectRemoved(string name)
        {
            Debug.Log("Remove object pose: " + name);
            GameObject.Destroy(m_ResultCube.gameObject);
            m_ResultCube = null;
        }
    }
}