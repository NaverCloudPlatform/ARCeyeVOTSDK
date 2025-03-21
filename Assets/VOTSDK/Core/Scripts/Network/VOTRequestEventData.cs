using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye.VOT
{
    public class VOTRequestEventData
    {
        public long Timestamp { get; private set; }
        public string Url { get; private set; }
        public string SecretKey { get; private set; }
        public string RequestBody { get; private set; }

        private static Texture2D s_RequestTexture = null;
        public Texture2D RequestTexture => s_RequestTexture;



        public static VOTRequestEventData Create(VOTRequestBody requestBody, byte[] imageBuffer)
        {
            VOTRequestEventData eventData = new VOTRequestEventData();
            eventData.Initialize(requestBody, imageBuffer);
            return eventData;
        }

        private void Initialize(VOTRequestBody requestBody, byte[] imageBuffer)
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

            if (texture.LoadImage(imageBuffer))
            {
                Initialize(requestBody, texture);
            }
            else
            {
                Debug.LogError("[VOTRequestEventData] Initialize - LoadImage failed!");
            }

            Object.Destroy(texture);
        }

        private void Initialize(VOTRequestBody requestBody, Texture2D requestTexture)
        {
            Timestamp = requestBody.timestamp;
            Url = requestBody.invokeUrl;
            SecretKey = requestBody.secretKey;
            RequestBody = requestBody.ToString();

            if (s_RequestTexture == null || s_RequestTexture.width != requestTexture.width)
            {
                s_RequestTexture = new Texture2D(requestTexture.width, requestTexture.height, requestTexture.format, false);
            }

            Graphics.CopyTexture(requestTexture, s_RequestTexture);
        }
    }

}