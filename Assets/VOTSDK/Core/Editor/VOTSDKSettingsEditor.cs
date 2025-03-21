using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ARCeye.VOT
{
    [CustomEditor(typeof(VOTSDKSettings))]
    public class VLSDKSettingsEditor : Editor
    {
        private VOTSDKSettings m_VOTSDKSettings;

        private SerializedProperty m_URLListProp;
        private SerializedProperty m_RequestIntervalProp;
        private SerializedProperty m_PoseFilterCapacityProp;
        private SerializedProperty m_LogLevelProp;


        void OnEnable()
        {
            m_VOTSDKSettings = (VOTSDKSettings)target;

            m_URLListProp = serializedObject.FindProperty("m_URLList");
            m_RequestIntervalProp = serializedObject.FindProperty("m_RequestInterval");
            m_PoseFilterCapacityProp = serializedObject.FindProperty("m_PoseFilterCapacity");
            m_LogLevelProp = serializedObject.FindProperty("m_LogLevel");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawLogo();

            DrawVOTURLList();
            DrawVOTInterval();
            DrawPoseFilterCapacity();
            DrawLogLevel();
        }

        private void DrawLogo()
        {
            EditorGUILayout.Space();

            GUIStyle style = new GUIStyle();
            style.fixedHeight = 30;
            style.alignment = TextAnchor.MiddleCenter;

            GUILayout.Label(Resources.Load("Sprites/Logo") as Texture, style, GUILayout.ExpandWidth(true));

            EditorGUILayout.Space();
        }

        private void DrawVOTURLList()
        {
            EditorGUILayout.PropertyField(m_URLListProp);

            EditorUtility.SetDirty(m_URLListProp.serializedObject.targetObject);
            m_URLListProp.serializedObject.ApplyModifiedProperties();
        }

        private void DrawVOTInterval()
        {
            EditorGUILayout.PropertyField(m_RequestIntervalProp);

            EditorUtility.SetDirty(m_RequestIntervalProp.serializedObject.targetObject);
            m_RequestIntervalProp.serializedObject.ApplyModifiedProperties();
        }

        private void DrawPoseFilterCapacity()
        {
            EditorGUILayout.PropertyField(m_PoseFilterCapacityProp);

            EditorUtility.SetDirty(m_PoseFilterCapacityProp.serializedObject.targetObject);
            m_PoseFilterCapacityProp.serializedObject.ApplyModifiedProperties();
        }

        private void DrawLogLevel()
        {
            EditorGUILayout.PropertyField(m_LogLevelProp);

            EditorUtility.SetDirty(m_LogLevelProp.serializedObject.targetObject);
            m_LogLevelProp.serializedObject.ApplyModifiedProperties();
        }
    }
}