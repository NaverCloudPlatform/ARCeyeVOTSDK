using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye.VOT
{
    public class VOTFrame
    {
        // uuid는 VOTFrame 최초 접근 시 한 번만 생성.
        static private string s_UUID;
        public string uuid
        {
            get
            {
                if (s_UUID == null)
                {
                    s_UUID = System.Guid.NewGuid().ToString(); ;
                }
                return s_UUID;
            }
        }

        public Vector3 position;
        public Quaternion rotation;

        public float fx;
        public float fy;
        public float cx;
        public float cy;

        public Texture imageTexture;
        public Matrix4x4 texMatrix = Matrix4x4.identity;


        private VOTFrame()
        {

        }

        public static VOTFrameBuilder Builder()
        {
            VOTFrameBuilder builder = new VOTFrameBuilder();
            return builder;
        }


        public class VOTFrameBuilder
        {
            private VOTFrame votFrame;
            private Camera m_MainCamera;
            private bool m_UseCustomePose = false;

            public VOTFrameBuilder()
            {
                votFrame = new VOTFrame();
            }

            public VOTFrameBuilder SetCameraPose(Vector3 position, Quaternion rotation)
            {
                votFrame.position = position;
                votFrame.rotation = rotation;
                m_UseCustomePose = true;
                return this;
            }

            public VOTFrameBuilder SetCameraParameters(float fx, float fy, float cx, float cy)
            {
                votFrame.fx = fx;
                votFrame.fy = fy;
                votFrame.cx = cx;
                votFrame.cy = cy;
                return this;
            }

            public VOTFrameBuilder SetImageBuffer(Texture texture)
            {
                votFrame.imageTexture = texture;
                return this;
            }

            public VOTFrameBuilder SetImageBuffer(Texture texture, Matrix4x4 texMatrix)
            {
                votFrame.imageTexture = texture;
                votFrame.texMatrix = texMatrix;
                return this;
            }

            public VOTFrame Build()
            {
                if (!m_UseCustomePose)
                {
                    if (m_MainCamera == null)
                    {
                        m_MainCamera = Camera.main;
                    }
                    votFrame.position = m_MainCamera.transform.position;
                    votFrame.rotation = m_MainCamera.transform.rotation;
                }

                if (votFrame.fx == 0 || votFrame.fy == 0 || votFrame.cx == 0 || votFrame.cy == 0)
                {
                    LogViewer.DebugLog(LogLevel.WARNING, "VOTFrame 요청 시 사용 된 camera param 값이 비정상임.");
                }

                if (votFrame.imageTexture == null)
                {
                    LogViewer.DebugLog(LogLevel.WARNING, "VOTFrame에 imageTexture가 할당되지 않았음");
                }

                return votFrame;
            }
        }
    }
}