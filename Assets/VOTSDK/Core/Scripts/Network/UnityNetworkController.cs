using AOT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ARCeye.VOT
{
    public class UnityNetworkController : NetworkController
    {
        //
        //  Native Bridge
        //

        public delegate void RequestVOTDelegate(RequestVOTInfo requestInfo);

        [DllImport(NativeBridge.dll)]
        private static extern void VOTSDK_SetRequestFuncNative(RequestVOTDelegate func);


        //
        //  Static Fields
        //

        static private Texture2D s_QueryTexture = null;
        static private UnityNetworkController s_Instance;


        //
        //  Inspector Fields
        //

        [SerializeField]
        private bool m_SaveQueryImage = false;
        public bool SaveQueryImage
        {
            get => m_SaveQueryImage;
            set => m_SaveQueryImage = value;
        }


        //
        //  요청 과부하 방지 관련.
        //

        private const int m_FullQueueWaitingSeconds = 5;
        private bool m_CheckQueueFullDuration = false;
        private DateTime m_FirstQueueFullTime;
        private Coroutine m_RequestCoroutine = null;
        private List<Coroutine> m_RequestCoroutines = new List<Coroutine>();


        //
        //  Implementations
        //

        private void Awake()
        {
            s_Instance = this;

            VOTSDK_SetRequestFuncNative(OnRequest);
        }


        // Native 영역에서 VOT 요청.
        // 요청 결과로 임의의 성공 응답을 Native로 전달.
        [MonoPInvokeCallback(typeof(RequestVOTDelegate))]
        unsafe private static void OnRequest(RequestVOTInfo requestInfo)
        {
            if (requestInfo.rawImage != IntPtr.Zero && !CreateQueryTexture(requestInfo.rawImage, requestInfo.texMatrix))
            {
                Debug.LogError("Failed to create a query texture");
                return;
            }

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                var responseEventData = VOTResponseEventData.Create(ResponseStatus.NetworkConnectionError);
                s_Instance.OnVOTResponded?.Invoke(responseEventData);

                Debug.LogError("Network is not reachable");
                return;
            }

            VOTRequestBody body = VOTRequestBody.Create(requestInfo);

            s_Instance.OnSendingRequestAsync(body, s_QueryTexture);
        }

        private static bool CreateQueryTexture(IntPtr rawImage, float[] texMatrix)
        {
            Matrix4x4 displayMatrix = new Matrix4x4();

            // 원본 이미지를 crop 하지 않고 그대로 사용.
            for (int i = 0; i < texMatrix.Length; i++)
            {
                int sign = texMatrix[i] < 0 ? -1 : 1;
                if (texMatrix[i] != 0)
                {
                    texMatrix[i] = sign;
                }
            }

            displayMatrix.SetRow(0, new Vector4(texMatrix[0], texMatrix[1], texMatrix[2], texMatrix[3]));
            displayMatrix.SetRow(1, new Vector4(texMatrix[4], texMatrix[5], texMatrix[6], texMatrix[7]));
            displayMatrix.SetRow(2, new Vector4(texMatrix[8], texMatrix[9], texMatrix[10], texMatrix[11]));
            displayMatrix.SetRow(3, new Vector4(texMatrix[12], texMatrix[13], texMatrix[14], texMatrix[15]));

            object texObj = GCHandle.FromIntPtr(rawImage).Target;
            Type texType = texObj.GetType();

            if (texType == typeof(Texture2D))
            {
                s_QueryTexture = texObj as Texture2D;
                s_QueryTexture = ImageUtility.RotateTexture(s_QueryTexture, displayMatrix);
                return true;
            }
            else if (texType == typeof(RenderTexture))
            {
                RenderTexture tex = texObj as RenderTexture;
                ImageUtility.ConvertRenderTextureToTexture2D(tex, ref s_QueryTexture);
                s_QueryTexture = ImageUtility.RotateTexture(s_QueryTexture, displayMatrix);
                return true;
            }
            else
            {
                Debug.LogError("Invalid type of texture is used");
                return false;
            }
        }

        private void OnSendingRequestAsync(VOTRequestBody body, Texture texture, int asyncCount = 20)
        {
            if (m_RequestCoroutines.Count >= asyncCount)
            {
                LogViewer.DebugLog(LogLevel.DEBUG, $"VOT 요청 대기열이 가득 찼습니다. 현재 요청을 무시됩니다. (대기열 크기 = {asyncCount})");
                CheckRequestQueueCapacity();
            }
            else
            {
                var c = StartCoroutine(Upload(body, texture));
                m_RequestCoroutines.Add(c);
            }
        }

        private void CheckRequestQueueCapacity()
        {
            if (!m_CheckQueueFullDuration)
            {
                // 대기열이 가득 찬 상태가 5초 이상 유지될 경우 대기열 모두 초기화.
                m_FirstQueueFullTime = DateTime.Now;
                m_CheckQueueFullDuration = true;
            }
            else
            {
                TimeSpan currTime = DateTime.Now.TimeOfDay;
                TimeSpan diff = currTime - m_FirstQueueFullTime.TimeOfDay;
                if (diff.Seconds >= m_FullQueueWaitingSeconds)
                {
                    LogViewer.DebugLog(LogLevel.DEBUG, $"VL 요청 대기열을 초기화합니다.");
                    m_CheckQueueFullDuration = false;
                    m_RequestCoroutines.Clear();
                }
            }
        }

        IEnumerator Upload(VOTRequestBody requestBody, Texture texture)
        {
            UnityWebRequest www = CreateRequest(requestBody, texture);

            if (m_SaveQueryImage)
            {
                ImageUtility.Save(requestBody.filename, requestBody.image);
            }

            // 요청 이벤트 전달.
            VOTRequestEventData eventData = VOTRequestEventData.Create(requestBody, requestBody.image);
            s_Instance.OnVOTRequested?.Invoke(eventData);

            LogViewer.DebugLog(LogLevel.DEBUG, "[NetworkController] " + requestBody.ToString());

            yield return www.SendWebRequest();

            VOTResponseEventData responseEventData = VOTResponseEventData.Create(www.downloadHandler.text, www.responseCode);
            s_Instance.OnVOTResponded?.Invoke(responseEventData);

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                LogViewer.DebugLog(LogLevel.WARNING, "[NetworkController] " + www.error);
            }
            else
            {
                LogViewer.DebugLog(LogLevel.DEBUG, "[NetworkController] " + www.downloadHandler.text);

                IntPtr msgPtr = Marshal.StringToHGlobalAnsi(www.downloadHandler.text);

                NativeBridge.SendResponseToNative(msgPtr);
            }

            m_RequestCoroutine = null;

            www.Dispose();

            if (m_RequestCoroutines.Count > 0)
            {
                m_RequestCoroutines.RemoveAt(0);
            }
        }

        private UnityWebRequest CreateRequest(VOTRequestBody requestBody, Texture texture)
        {
            UnityWebRequest www = new UnityWebRequest();

            www.url = requestBody.invokeUrl;
            www.SetRequestHeader("X-ARCEYE-SECRET", requestBody.secretKey);

            requestBody.image = ImageUtility.ConvertToJpegData(texture);

            www.method = "POST";
            www.uploadHandler = GenerateUploadHandler(requestBody);
            www.downloadHandler = GenerateDownloadHandler();

            return www;
        }

        private UploadHandler GenerateUploadHandler(VOTRequestBody requestBody)
        {
            byte[] boundary = UnityWebRequest.GenerateBoundary();
            byte[] body = GenerateBodyBuffer(requestBody, boundary);

            UploadHandler uploader = new UploadHandlerRaw(body);
            string contentType = String.Concat("multipart/form-data; boundary=", Encoding.UTF8.GetString(boundary));
            uploader.contentType = contentType;

            return uploader;
        }

        private byte[] GenerateBodyBuffer(VOTRequestBody requestBody, byte[] boundary)
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormFileSection(requestBody.imageFieldName, requestBody.image, requestBody.filename, "image/jpeg"));
            foreach (var param in requestBody.parameters)
            {
                formData.Add(new MultipartFormDataSection(param.Key, param.Value));
            }

            byte[] formSections = UnityWebRequest.SerializeFormSections(formData, boundary);
            byte[] terminate = Encoding.UTF8.GetBytes(String.Concat("\r\n--", Encoding.UTF8.GetString(boundary), "--"));
            byte[] body = new byte[formSections.Length + terminate.Length];

            Buffer.BlockCopy(formSections, 0, body, 0, formSections.Length);
            Buffer.BlockCopy(terminate, 0, body, formSections.Length, terminate.Length);

            return body;
        }

        private DownloadHandler GenerateDownloadHandler()
        {
            return new DownloadHandlerBuffer();
        }
    }
}