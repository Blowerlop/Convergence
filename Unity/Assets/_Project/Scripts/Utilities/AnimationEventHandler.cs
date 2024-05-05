using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Project
{
    public class AnimationEventHandler : SerializedMonoBehaviour
    {
        [SerializeField] private Dictionary<string, UnityEvent> events  = new();
        
        // Had to change the name of the method because NetworkAnimator throw an error
        // --> 'Unity Failed to call AnimationEvent Invoke of class NetworkAnimator.
        // The function must have either 0 or 1 parameters and the parameter
        // can only be: string, float, int, enum, Object and AnimationEvent."
        //
        // Maybe related to https://forum.unity.com/threads/animation-events-problems.723623/
        public void Callback(string id)
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
