using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ARCeye.VOT
{
    public class VOTSDKMenu
    {
        [MenuItem("GameObject/ARC-eye/VOT SDK/Create VOTSDKManager")]
        private static void CreateVOTSDKManager()
        {
            if (CheckIsVOTSDKManagerExisting())
            {
                Debug.LogWarning("VOTSDKManager is already added");
                return;
            }

            string[] settingGUIDInAssets = AssetDatabase.FindAssets("t:VOTSDKSettings");

            if (settingGUIDInAssets.Length == 0)
            {
                Debug.LogWarning("Failed to find VOTSDKSettings file. Please reimport VOTSDK package");
                return;
            }

            // Create VOTSDKManager
            VOTSDKManager VOTSDKManager = VOTSDKManagerFactory.CreateVOTSDKManager();

            // Assign VOTSDKSettings
            string settingGUID = settingGUIDInAssets[0];
            string settingsPath = AssetDatabase.GUIDToAssetPath(settingGUID);
            VOTSDKSettings settings = (VOTSDKSettings)AssetDatabase.LoadAssetAtPath(settingsPath, typeof(VOTSDKSettings));
            VOTSDKManager.settings = settings;
        }

        private static bool CheckIsVOTSDKManagerExisting()
        {
            var VOTSDKManager = GameObject.FindObjectOfType<VOTSDKManager>();
            return VOTSDKManager != null;
        }
    }
}