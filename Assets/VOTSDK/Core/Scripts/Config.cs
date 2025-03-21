using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ARCeye.VOT
{
    [System.Serializable]
    public class Config
    {
        [SerializeField]
        private VOTURL[] m_URLList;
        public VOTURL[] urlList
        {
            get => m_URLList;
            set => m_URLList = value;
        }

        [SerializeField]
        private int m_RequestInterval = 300;
        public int requestInterval
        {
            get => m_RequestInterval;
            set => m_RequestInterval = value;
        }

        [SerializeField]
        private int m_PoseFilterCapacity = 1;
        public int poseFilterCapacity
        {
            get => m_PoseFilterCapacity;
            set => m_PoseFilterCapacity = value;
        }

        [SerializeField]
        private LogLevel m_LogLevel = LogLevel.DEBUG;
        public LogLevel logLevel
        {
            get => m_LogLevel;
            set => m_LogLevel = value;
        }

        public Config(VOTSDKSettings settings)
        {
            m_URLList = settings.urlList.ToArray();
            m_RequestInterval = settings.requestInterval;
            m_PoseFilterCapacity = settings.poseFilterCapacity;
            m_LogLevel = settings.logLevel;
        }
    }
}
