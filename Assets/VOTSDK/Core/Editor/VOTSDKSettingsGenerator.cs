using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ARCeye.VOT
{
    public class VOTSDKSettingsGenerator
    {
        [MenuItem("Assets/Create/ARCeye/VOTSDKSettings")]
        public static void CreateVLSDKSettings()
        {
            VOTSDKSettings asset = ScriptableObject.CreateInstance<VOTSDKSettings>();

            AssetDatabase.CreateAsset(asset, "Assets/VOTSDK/VOTSDKSettings.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
    }
}