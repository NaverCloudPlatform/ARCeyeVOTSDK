using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye.VOT
{
    public class VOTResponseEventData
    {
        public ResponseStatus Status { get; private set; }
        public long Timestamp { get; private set; }
        public int ObjectsCount { get; set; }
        public string ResponseBody { get; private set; }

        public static VOTResponseEventData Create(ResponseStatus status)
        {
            VOTResponseEventData eventData = new VOTResponseEventData();
            eventData.Status = status;
            return eventData;
        }

        public static VOTResponseEventData Create(string responseBody, long code)
        {
            VOTResponseEventData eventData = new VOTResponseEventData();

            if (code != 200)
            {
                eventData.Status = ConvertToResponseStatus(code);
            }
            else
            {
                VOTResponseParser parser = new VOTResponseParser();
                parser.Parse(responseBody);

                eventData.Status = parser.Status;
                eventData.Timestamp = parser.Timestamp;
                eventData.ObjectsCount = parser.ObjectsCount;
                eventData.ResponseBody = responseBody;
            }

            return eventData;
        }

        private static ResponseStatus ConvertToResponseStatus(long code)
        {
            switch (code)
            {
                case 400:
                    return ResponseStatus.BadRequestServer;
                case 404:
                    return ResponseStatus.ServerNotFound;
                case 401:
                    return ResponseStatus.Unauthorized;
                case 500:
                    return ResponseStatus.InternalServerError;
                default:
                    return ResponseStatus.UnknownError;
            }
        }
    }
}