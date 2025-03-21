using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ARCeye.VOT
{
    [System.Serializable]
    public class VOTURL
    {
        [SerializeField]
        private string m_Name;
        public string name
        {
            get => m_Name;
            set => m_Name = value;
        }

        [SerializeField]
        private string m_InvokeUrl;
        public string invokeUrl
        {
            get => m_InvokeUrl;
            set => m_InvokeUrl = value;
        }

        [SerializeField]
        private string m_SecretKey;
        public string secretKey
        {
            get => m_SecretKey;
            set => m_SecretKey = value;
        }
    }
}