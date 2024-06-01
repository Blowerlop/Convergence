using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project
{
    [System.Serializable]
    public class TimeStamp
    {
        public float Timestamp;
        public string Message; 
    }
    [System.Serializable]
    public class AudioHelperItem 
    {
        public StudioEventEmitter eventEmitter;
        public string eventName; 
        public List<TimeStamp> timeStamps; 

    }
}
