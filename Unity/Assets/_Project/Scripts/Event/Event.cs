using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Project
{
    [System.Serializable]
    public class Event
    {
        // [SerializeField] public UnityEvent z = new UnityEvent();
        private readonly string _eventName;

        private Action _action = delegate {  };
        private readonly List<Action> _actionsTrackList = new List<Action>();
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        private readonly Dictionary<Action, object> _actionTrackListExpend = new Dictionary<Action, object>();
        #endif

        private bool sendLog = false;
        
        
        public Event(string eventName, bool sendLog = true)
        {
            _eventName = eventName;
            this.sendLog = sendLog;
        }

        // public void Init()
        // {
        //     int persistentEventCount = z.GetPersistentEventCount();
        //     for (int i = 0; i < persistentEventCount; i++)
        //     {
        //         Object targetObject = z.GetPersistentTarget(i);
        //         string methodName = z.GetPersistentMethodName(i);
        //         MethodInfo methodInfo = UnityEvent.GetValidMethodInfo(targetObject, methodName, new Type[] {typeof(float)});
        //         // Subscribe(methodInfo.Invoke(targetObject, new object[] { 180f }));
        //         // methodInfo.Invoke(targetObject, new object[] { 180f });
        //         // Debug.Log(method);
        //         
        //         
        //         // https://gist.github.com/wesleywh/1c56d880c0289371ea2dc47661a0cdaf
        //     }
        //     
        //     // To do : use reflection to be able to get internal unity event function
        // }
        
        
        public void Invoke(object sender, bool debugCallback = true) 
        {
            if (sendLog) Debug.Log($"<color=#00FF00>{sender} invoked {_eventName}</color>");
            _action.Invoke();
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (debugCallback == false) return;
            
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"<color=#00FF00>Methods called by {_eventName}:</color> \n");

            foreach (var kvp in _actionTrackListExpend)
            {
                stringBuilder.Append($"<color=#00FF00>- {kvp.Key.Method.Name} --- by {kvp.Value}</color> \n");

            }
            Debug.Log(stringBuilder);
            #endif
            
        }
        
        public void Subscribe(object subscriber, Action action)
        {
            if (IsListenerAlreadySubscribe(action))
            {
                Debug.LogError($"Method - {action.Method.Name} - is already registered in the event");
            }
            else
            {
                _action += action;
                _actionsTrackList.Add(action);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                _actionTrackListExpend.Add(action, subscriber);
#endif
                
                if (sendLog) Debug.Log($"Method - {action.Method.Name} - has subscribed");
            }
        }

        public void Unsubscribe(Action action)
        {
            if (IsListenerAlreadySubscribe(action) == false)
            {
                Debug.LogWarning($"Method - {action.Method.Name} - is not registered in the event");
            }
            else
            {
                _action -= action;
                _actionsTrackList.Remove(action);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                _actionTrackListExpend.Remove(action);
#endif
                
                if (sendLog) Debug.Log($"Method - {action.Method.Name} - has unsubscribed");
            }
        }

        private bool IsListenerAlreadySubscribe(Action action)
        {
            return _actionsTrackList.Contains(action);
        }
    }
    
    public class Event<T>
    {
        private readonly string _eventName;

        private Action<T> _action = delegate {  };
        private readonly List<Action<T>> _actionsTrackList = new List<Action<T>>();
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        private readonly Dictionary<Action<T>, object> _actionTrackListExpend = new Dictionary<Action<T>, object>();
        #endif
        
        
        public Event(string eventName)
        {
            _eventName = eventName;
        }
        
        
        public void Invoke(object sender, bool debugCallback, T arg ) 
        {
            if (debugCallback) Debug.Log($"<color=#00FF00>{sender} invoked {_eventName}</color>"); 
            _action.Invoke(arg);
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (debugCallback == false) return;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"<color=#00FF00>Methods called by {_eventName}:</color> \n");

            foreach (var kvp in _actionTrackListExpend)
            {
                stringBuilder.Append($"<color=#00FF00>- {kvp.Key.Method.Name} --- by {kvp.Value}</color> \n");

            }
            Debug.Log(stringBuilder);
            #endif
            
        }

        public void Subscribe(object subscriber, Action<T> action)
        {
            if (IsListenerAlreadySubscribe(action))
            {
                Debug.LogError($"Method - {action.Method.Name} - is already registered in the event");
            }
            else
            {
                _action += action;
                _actionsTrackList.Add(action);
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                _actionTrackListExpend.Add(action, subscriber);
                #endif
                
                Debug.Log($"Method - {action.Method.Name} - has subscribed");
            }
        }

        public void Unsubscribe(Action<T> action)
        {
            if (IsListenerAlreadySubscribe(action) == false)
            {
                Debug.LogWarning($"Method - {action.Method.Name} - is not registered in the event");
            }
            else
            {
                _action -= action;
                _actionsTrackList.Remove(action);
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                _actionTrackListExpend.Remove(action);
                #endif
                
                Debug.Log($"Method - {action.Method.Name} - has unsubscribed");
            }
        }

        private bool IsListenerAlreadySubscribe(Action<T> action)
        {
            return _actionsTrackList.Contains(action);
        }
    }
    
    public class Event<T1, T2>
    {
        private readonly string _eventName;

        private Action<T1, T2> _action = delegate {  };
        private readonly List<Action<T1, T2>> _actionsTrackList = new List<Action<T1, T2>>();
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        private readonly Dictionary<Action<T1, T2>, object> _actionTrackListExpend = new Dictionary<Action<T1, T2>, object>();
        #endif
        
        
        public Event(string eventName)
        {
            _eventName = eventName;
        }
        
        
        public void Invoke(object sender, bool debugCallback, T1 arg1, T2 arg2) 
        {
            Debug.Log($"<color=#00FF00>{sender} invoked {_eventName}</color>");
            _action.Invoke(arg1, arg2);
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (debugCallback == false) return;
            
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"<color=#00FF00>Methods called by {_eventName}:</color> \n");

            foreach (var kvp in _actionTrackListExpend)
            {
                stringBuilder.Append($"<color=#00FF00>- {kvp.Key.Method.Name} --- by {kvp.Value}</color> \n");

            }
            Debug.Log(stringBuilder);
#endif
        }

        public void Subscribe(object subscriber, Action<T1, T2> action)
        {
            if (IsListenerAlreadySubscribe(action))
            {
                Debug.LogError($"Method - {action.Method.Name} - is already registered in the event");
            }
            else
            {
                _action += action;
                _actionsTrackList.Add(action);
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                _actionTrackListExpend.Add(action, subscriber);
                #endif
                
                Debug.Log($"Method - {action.Method.Name} - has subscribed");
            }
        }

        public void Unsubscribe(Action<T1, T2> action)
        {
            if (IsListenerAlreadySubscribe(action) == false)
            {
                Debug.LogWarning($"Method - {action.Method.Name} - is not registered in the event");
            }
            else
            {
                _action -= action;
                _actionsTrackList.Remove(action);
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                _actionTrackListExpend.Remove(action);
                #endif
                
                Debug.Log($"Method - {action.Method.Name} - has unsubscribed");
            }
        }

        private bool IsListenerAlreadySubscribe(Action<T1, T2> action)
        {
            return _actionsTrackList.Contains(action);
        }
    }
    
    public class Event<T1, T2, T3>
    {
        private readonly string _eventName;

        private Action<T1, T2, T3> _action = delegate {  };
        private List<Action<T1, T2, T3>> _actionsTrackList = new List<Action<T1, T2, T3>>();
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        private Dictionary<Action<T1, T2, T3>, object> _actionTrackListExpend = new Dictionary<Action<T1, T2, T3>, object>();
        #endif
        
        
        public Event(string eventName)
        {
            _eventName = eventName;
        }
        
        
        public void Invoke(object sender, bool debugCallback, T1 arg1, T2 arg2, T3 arg3) 
        {
            Debug.Log($"<color=#00FF00>{sender} invoked {_eventName}</color>");
            _action.Invoke(arg1, arg2, arg3);
            
             #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (debugCallback == false) return;
            
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"<color=#00FF00>Methods called by {_eventName}:</color> \n");

            foreach (var kvp in _actionTrackListExpend)
            {
                stringBuilder.Append($"<color=#00FF00>- {kvp.Key.Method.Name} --- by {kvp.Value}</color> \n");

            }
            Debug.Log(stringBuilder);
            #endif
            
        }

        
        public void Subscribe(object subscriber, Action<T1, T2, T3> action)
        {
            if (IsListenerAlreadySubscribe(action))
            {
                Debug.LogError($"Method - {action.Method.Name} - is already registered in the event");
            }
            else
            {
                _action += action;
                _actionsTrackList.Add(action);
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                _actionTrackListExpend.Add(action, subscriber);
                #endif
                
                Debug.Log($"Method - {action.Method.Name} - has subscribed");
            }
        }

        public void Unsubscribe(Action<T1, T2, T3> action)
        {
            if (IsListenerAlreadySubscribe(action) == false)
            {
                Debug.LogWarning($"Method - {action.Method.Name} - is not registered in the event");
            }
            else
            {
                _action -= action;
                _actionsTrackList.Remove(action);
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                _actionTrackListExpend.Remove(action);
                #endif
                
                Debug.Log($"Method - {action.Method.Name} - has unsubscribed");
            }
        }

        private bool IsListenerAlreadySubscribe(Action<T1, T2, T3> action)
        {
            return _actionsTrackList.Contains(action);
        }
    }
    
    public class Event<T1, T2, T3, T4>
    {
        private readonly string _eventName;

        private Action<T1, T2, T3, T4> _action = delegate {  };
        private List<Action<T1, T2, T3, T4>> _actionsTrackList = new List<Action<T1, T2, T3, T4>>();
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        private Dictionary<Action<T1, T2, T3, T4>, object> _actionTrackListExpend = new Dictionary<Action<T1, T2, T3, T4>, object>();
        #endif
        
        
        public Event(string eventName)
        {
            _eventName = eventName;
        }
        
        
        public void Invoke(object sender, bool debugCallback, T1 arg1, T2 arg2, T3 arg3, T4 arg4) 
        {
            Debug.Log($"<color=#00FF00>{sender} invoked {_eventName}</color>");
            _action.Invoke(arg1, arg2, arg3, arg4);
            
             #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (debugCallback == false) return;
            
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"<color=#00FF00>Methods called by {_eventName}:</color> \n");

            foreach (var kvp in _actionTrackListExpend)
            {
                stringBuilder.Append($"<color=#00FF00>- {kvp.Key.Method.Name} --- by {kvp.Value}</color> \n");

            }
            Debug.Log(stringBuilder);
            #endif
            
        }
        
        public void Subscribe(object subscriber, Action<T1, T2, T3, T4> action)
        {
            if (IsListenerAlreadySubscribe(action))
            {
                Debug.LogError($"Method - {action.Method.Name} - is already registered in the event");
            }
            else
            {
                _action += action;
                _actionsTrackList.Add(action);
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                _actionTrackListExpend.Add(action, subscriber);
                #endif
                
                Debug.Log($"Method - {action.Method.Name} - has subscribed");
            }
        }

        public void Unsubscribe(Action<T1, T2, T3, T4> action)
        {
            if (IsListenerAlreadySubscribe(action) == false)
            {
                Debug.LogWarning($"Method - {action.Method.Name} - is not registered in the event");
            }
            else
            {
                _action -= action;
                _actionsTrackList.Remove(action);
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                _actionTrackListExpend.Remove(action);
                #endif
                
                Debug.Log($"Method - {action.Method.Name} - has unsubscribed");
            }
        }

        private bool IsListenerAlreadySubscribe(Action<T1, T2, T3, T4> action)
        {
            return _actionsTrackList.Contains(action);
        }
    }
}