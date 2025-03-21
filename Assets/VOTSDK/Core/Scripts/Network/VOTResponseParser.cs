using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace ARCeye.VOT
{
    public class VOTResponseParser
    {
        private JObject m_RootObject;

        public ResponseStatus Status { get; private set; }
        public long Timestamp { get; private set; }
        public int ObjectsCount { get; private set; }
        public string ResponseBody { get; private set; }


        public void Parse(string responseBody)
        {
            m_RootObject = JObject.Parse(responseBody);

            if (m_RootObject.ContainsKey("timestamp"))
            {
                Timestamp = m_RootObject["timestamp"].Value<long>();
            }

            // VOT 에러코드 (detector 하위의 detect)
            // https://yona.naverlabs.com/ARNavi/VOT/issue/734
            if (m_RootObject.ContainsKey("status"))
            {
                int code = m_RootObject["status"].Value<int>();
                if (code == 0)
                {
                    Status = ResponseStatus.Success;
                }
                else if (
                   code == 53 ||  // No object
                   code == 55 ||  // No pose connection
                   code == 60 ||  // Invalid pose response
                   code == 61     // Inadequate pose result
                   )
                {
                    Status = ResponseStatus.Failed;
                }
                else
                {
                    Status = ResponseStatus.UnknownError;
                }
            }

            if (m_RootObject.ContainsKey("objects"))
            {
                JArray objects = m_RootObject["objects"].Value<JArray>();
                ObjectsCount = objects.Count;
            }
        }
    }
}