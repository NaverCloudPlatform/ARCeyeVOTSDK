using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ARCeye.VOT
{
    public class NetworkController : MonoBehaviour
    {
        public UnityEvent<VOTRequestEventData> OnVOTRequested { get; set; }
        public UnityEvent<VOTResponseEventData> OnVOTResponded { get; set; }
    }
}
