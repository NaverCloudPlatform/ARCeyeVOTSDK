using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye.VOT
{
    public class VOTRequestBody
    {
        public long timestamp;
        public string uuid;
        public string invokeUrl;
        public string secretKey;
        public string filename;
        public string imageFieldName;
        public byte[] image;
        public Dictionary<string, string> parameters = new Dictionary<string, string>();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var elem in parameters)
            {
                sb.Append($"{elem.Key} : {elem.Value}\n");
            }

            return @$"
            InvokeUrl - {invokeUrl}
            SecretKey - {secretKey}
            filename - {filename}
            params - {sb.ToString()}
            ";
        }

        public static bool IsValidIntrinsic(string paramStr)
        {
            if (String.IsNullOrEmpty(paramStr))
                return false;

            // row major 배열 여부 검사.
            string[] elems = paramStr.Split(",");
            return float.Parse(elems[0]) != 0 && float.Parse(elems[1]) == 0 &&
                float.Parse(elems[2]) != 0 && float.Parse(elems[3]) == 0;
        }

        public static bool IsValidExtrinsic(string paramStr)
        {
            if (String.IsNullOrEmpty(paramStr))
            {
                Debug.LogWarning("[VOTRequestBody] IsValidExtrinsic - extrinsic is empty!");
                return false;
            }

            // row major 배열 여부 검사.
            string[] elems = paramStr.Split(",");
            bool res = float.Parse(elems[12]) == 0 && float.Parse(elems[13]) == 0 &&
                        float.Parse(elems[14]) == 0 && float.Parse(elems[15]) == 1;

            if (!res)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        sb.Append(elems[i * 4 + j]).Append(" ");
                    }
                    sb.Append("\n");
                }
                Debug.LogWarning("[VOTRequestBody] IsValidExtrinsic - extrinsic is not row major\n" + sb.ToString());

                return false;
            }
            else
            {
                return true;
            }
        }

        public static VOTRequestBody Create(ARCeye.VOT.RequestVOTInfo requestInfo)
        {
            if (IsARCeyeURL(requestInfo.invokeUrl))
            {
                return CreateARCeyeRequest(requestInfo);
            }
            else
            {
                return CreateLABSRequest(requestInfo);
            }
        }

        private static bool IsARCeyeURL(string url)
        {
            string arceyeUrl = "arc-eye.ncloud.com";
            return url.Contains(arceyeUrl);
        }

        private static VOTRequestBody CreateARCeyeRequest(ARCeye.VOT.RequestVOTInfo requestInfo)
        {
            VOTRequestBody body = new VOTRequestBody();

            body.uuid = requestInfo.uuid;
            body.invokeUrl = requestInfo.invokeUrl;
            body.secretKey = requestInfo.secretKey;
            body.filename = "query.jpg";
            body.imageFieldName = "image";

            body.parameters.Add("uuid", requestInfo.uuid);
            body.parameters.Add("distort", requestInfo.distortion);
            body.parameters.Add("timestamp", requestInfo.timestamp);

            if (VOTRequestBody.IsValidIntrinsic(requestInfo.intrinsic))
            {
                body.parameters.Add("intrinsic", requestInfo.intrinsic);
            }

            if (VOTRequestBody.IsValidExtrinsic(requestInfo.extrinsic))
            {
                body.parameters.Add("extrinsic", requestInfo.extrinsic);
            }

            return body;
        }

        private static VOTRequestBody CreateLABSRequest(ARCeye.VOT.RequestVOTInfo requestInfo)
        {
            VOTRequestBody body = new VOTRequestBody();

            body.uuid = requestInfo.uuid;
            body.invokeUrl = requestInfo.invokeUrl;
            body.secretKey = requestInfo.secretKey;
            body.filename = "query.jpg";
            body.imageFieldName = "image";

            body.parameters.Add("uuid", requestInfo.uuid);
            body.parameters.Add("distort", requestInfo.distortion);
            body.parameters.Add("timestamp", requestInfo.timestamp);

            if (VOTRequestBody.IsValidIntrinsic(requestInfo.intrinsic))
            {
                body.parameters.Add("intrinsic", requestInfo.intrinsic);
            }

            if (VOTRequestBody.IsValidExtrinsic(requestInfo.extrinsic))
            {
                body.parameters.Add("extrinsic", requestInfo.extrinsic);
            }

            return body;
        }
    }
}