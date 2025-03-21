using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye.VOT
{
    public struct DetectedObjectData
    {
        public int status;
        public float probability;

        public string name;

        public Vector3 position;
        public Quaternion rotation;
        public Vector3 size;

        public List<Vector3> corners3d;

        public override string ToString()
        {
            return $"name : {name}\n" +
                   $"probability : {probability}\n" +
                   $"potision : {position}\n" +
                   $"rotation : {rotation}";
        }
    }
}