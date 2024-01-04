using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Project
{
    public class AnimationEventHandler : SerializedMonoBehaviour
    {
        [SerializeField] private Dictionary<string, UnityEvent> events  = new();
        
        public void Invoke(string id)
        {
            if (!events.ContainsKey(id))
            {
                Debug.LogError($"Can't invoke AnimationEvent {id} because it is not registered.");
                return;
            }
            
            events[id]?.Invoke();
        }
    }
}
