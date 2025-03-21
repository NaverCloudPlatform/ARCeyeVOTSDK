using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ARCeye.VOT
{
    public class VOTSDKManagerFactory : MonoBehaviour
    {
        public static VOTSDKManager CreateVOTSDKManager()
        {
            // Create VOTSDKManager
#if UNITY_EDITOR
            GameObject VOTSDKManagerObject;
            if (EditorApplication.isPlaying)
            {
                VOTSDKManagerObject = new GameObject("VOTSDKManager");
                VOTSDKManagerObject.AddComponent<UnityNetworkController>();
                VOTSDKManagerObject.AddComponent<VOTSDKManager>();
                VOTSDKManagerObject.AddComponent<LogViewer>();
            }
            else
            {
                VOTSDKManagerObject = ObjectFactory.CreateGameObject("VOTSDKManager", typeof(VOTSDKManager));
                VOTSDKManagerObject.AddComponent<UnityNetworkController>();
                VOTSDKManagerObject.AddComponent<LogViewer>();
            }
#else
            GameObject VOTSDKManagerObject = new GameObject("VOTSDKManager");
            VOTSDKManagerObject.AddComponent<VOTSDKManager>();
#endif 

            return VOTSDKManagerObject.GetComponent<VOTSDKManager>();
        }
    }
}