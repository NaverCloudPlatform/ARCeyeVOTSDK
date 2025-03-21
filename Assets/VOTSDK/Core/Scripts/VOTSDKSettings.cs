using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye.VOT
{
    [System.Serializable]
    public class VOTSDKSettings : ScriptableObject
    {
        [field: SerializeField, Tooltip("The request URL generated through the integration of ARC eye's API")]
        private List<VOTURL> m_URLList = new List<VOTURL>();
        public List<VOTURL> urlList
        {
            get => m_URLList;
            set => m_URLList = value;
        }

        [SerializeField, Tooltip("Request interval (unit: ms)")]
        [Range(250, 3000)]
        private int m_RequestInterval = 300;
        public int requestInterval
        {
            get => m_RequestInterval;
            set => m_RequestInterval = value;
        }

        [SerializeField, Tooltip("Pose Filter Capacity")]
        [Range(1, 10)]
        private int m_PoseFilterCapacity = 5;
        public int poseFilterCapacity
        {
            get => m_PoseFilterCapacity;
            set => m_PoseFilterCapacity = value;
        }

        [SerializeField, Tooltip("Visualize the responsed VOT poses")]
        private bool m_ShowVOTPose = false;
        public bool showVOTPose
        {
            get => m_ShowVOTPose;
            set => m_ShowVOTPose = value;
        }

        [SerializeField, Tooltip("Set the log level for console output")]
        private LogLevel m_LogLevel = LogLevel.DEBUG;
        public LogLevel logLevel
        {
            get => m_LogLevel;
            set => m_LogLevel = value;
        }
    }
}