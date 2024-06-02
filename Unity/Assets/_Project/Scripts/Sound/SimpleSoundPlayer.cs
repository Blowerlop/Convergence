using System;
using System.Collections;
using System.Collections.Generic;
using Project._Project.Scripts.Managers;
using UnityEngine;

namespace Project
{
    public class SimpleSoundPlayer : MonoBehaviour
    {
        public string eventId, alias;
        public SoundManager.EventType type;
        public bool clearOnChange;
        
        void Start()
        {
            SoundManager.instance.PlayStaticSound(eventId, alias, null, type);
        }

        void Update()
        {
        
        }

        private void OnDestroy()
        {
            if (clearOnChange)
            {
                SoundManager.instance.StopStaticSound(alias);
            }
        }
    }
}
