using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARCeye.VOT
{
    public class ResultCube : MonoBehaviour
    {
        [SerializeField] private TextMeshPro m_NameText;
        [SerializeField] private Transform m_Axis;
        [SerializeField] private Transform[] m_Corners;


        private void Awake()
        {
            // gameObject.SetActive(false);
        }

        public void Initialize(DetectedObjectData detectedObject)
        {
            m_NameText.text = detectedObject.name;
            transform.position = detectedObject.position;
            transform.rotation = detectedObject.rotation;
            transform.localScale = detectedObject.size;

            // corner3d 시각화.
            for (int i = 0; i < detectedObject.corners3d.Count; i++)
            {
                m_Corners[i].position = detectedObject.corners3d[i];
                SetGlobalScale(m_Corners[i], Vector3.one * 0.03f);
            }

            SetGlobalScale(m_Axis, Vector3.one);
            SetGlobalScale(m_NameText.transform, Vector3.one);
        }

        void SetGlobalScale(Transform objTransform, Vector3 globalScale)
        {
            Vector3 parentScale = objTransform.parent != null ? objTransform.parent.lossyScale : Vector3.one;
            objTransform.localScale = new Vector3(
                globalScale.x / parentScale.x,
                globalScale.y / parentScale.y,
                globalScale.z / parentScale.z
            );
        }
    }
}