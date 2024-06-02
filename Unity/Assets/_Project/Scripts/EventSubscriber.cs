using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using OpCodes = System.Reflection.Emit.OpCodes;

namespace Project
{
    public enum EInitialization
    {
        Awake,
        Start,
        OnEnable,
        OnDisable,
        OnDestroy
    }
    
    public enum EDecommissioning
    {
        OnDisable,
        OnDestroy,
        Fire
    }
    
    public class EventSubscriber : SerializedMonoBehaviour
    {
        public const BindingFlags BINDING_FLAGS = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        [Title("Target")] 
        [SerializeField] private bool _searchByInstance = true;
        [SerializeField, ShowIf(nameof(_searchByInstance))] private MonoBehaviour _targetMonoBehaviour;
        [OdinSerialize, HideIf(nameof(_searchByInstance))] private Type _targetType;
        [OdinSerialize, ValueDropdown(nameof(GetEventsName)), ShowIf(nameof(GetTargetObject))] private string _memberInfoName;
        [NonSerialized] private MemberInfo _memberInfo;
        
        [Title("Execution Timing")]
        [SerializeField] private EInitialization _subscribeExecutionTiming;
        [SerializeField] private EDecommissioning _unsubscribeExecutionTiming;
        
        [Title("Callback")]
        [SerializeField] public UnityEvent _callback;
        [OdinSerialize, ReadOnly] private Delegate _delegate;


        private void Awake()
        {
            if (_subscribeExecutionTiming == EInitialization.Awake) Subscribe();
        }
        
        private void Start()
        {
            if (_subscribeExecutionTiming == EInitialization.Start) Subscribe();
        }
        
        private void OnEnable()
        {
            if (_subscribeExecutionTiming == EInitialization.OnEnable) Subscribe();
        }
        
        private void OnDisable()
        {
            if (_unsubscribeExecutionTiming == EDecommissioning.OnDisable) Unsubscribe();
        }
        
        private void OnDestroy()
        {
            if (_unsubscribeExecutionTiming == EDecommissioning.OnDestroy) Unsubscribe();
        }


        private void Subscribe()
        {
            if (string.IsNullOrEmpty(_memberInfoName)) return;
            
            _memberInfo = GetTargetType().GetEvent(_memberInfoName, GetBindingFlags());
            _memberInfo ??= GetTargetType().GetField(_memberInfoName, GetBindingFlags());
            
            if (_memberInfo == null) throw new Exception($"Event or Field {_memberInfoName} not found in {GetTargetType()}");

            UpdateDelegate();

            if (_memberInfo is EventInfo eventInfo)
            {
                eventInfo.AddEventHandler(GetTargetObject(), _delegate);
            }
            else if (_memberInfo is FieldInfo fieldInfo)
            {
                fieldInfo.FieldType.GetMethod("AddListener").Invoke(_targetMonoBehaviour.GetComponent(GetTargetObject().GetType()), new object[] { _delegate });
            }
        }
        
        private void Unsubscribe()
        {
            if (_memberInfo == null) throw new Exception($"Event or Field {_memberInfoName} not found in {GetTargetType()}");
            
            if (_memberInfo is EventInfo eventInfo) eventInfo.RemoveEventHandler(_targetMonoBehaviour, _delegate);
            else if (_memberInfo is FieldInfo fieldInfo)
            {
                fieldInfo.FieldType.GetMethod("RemoveListener").Invoke(_targetMonoBehaviour.GetComponent(GetTargetObject().GetType()), new object[] { _delegate });
            }
        }


        #region  Editor

        private IEnumerable<string> GetEventsName()
        {
            if (GetTargetObject() == null) return Enumerable.Empty<string>();

            Type targetType = GetTargetType();
            
            var eventFields = targetType.GetEvents(GetBindingFlags()).Cast<MemberInfo>();
            // var unityEventFields = targetType.GetFields(GetBindingFlags()).Where(x => x.FieldType.IsSubclassOf(typeof(UnityEventBase))).Cast<MemberInfo>();
            
            // Get name of the event
            // return eventFields.Concat(unityEventFields).Select(x => x.Name);
            return eventFields.Select(x => x.Name);
        }

        private Type GetTargetType()
        {
            if (_searchByInstance) return _targetMonoBehaviour.GetType();
            return _targetType;
        }
        
        private object GetTargetObject()
        {
            if (_searchByInstance) return _targetMonoBehaviour;
            return _targetType;
        }
        
        private BindingFlags GetBindingFlags()
        {
            if (_searchByInstance) return BINDING_FLAGS;
            return BINDING_FLAGS;
        }
        
        private void UpdateDelegate()
        {
            if (string.IsNullOrEmpty(_memberInfoName)) return;
            
            if (_memberInfo == null) throw new Exception($"Event or Field {_memberInfoName} not found in {GetTargetType()}");
            

            if (_memberInfo is EventInfo eventInfo) 
            {
                // Type tDelegate = eventInfo.EventHandlerType;
                //
                // DynamicMethod handler = new DynamicMethod("", null, GetDelegateParameterTypes(tDelegate));
                // ILGenerator ilgen = handler.GetILGenerator();
                //
                // MethodInfo simpleShow = typeof(EventSubscriber).GetMethod("InvokeUNity");
                //
                Type debugType = typeof(Debug);
                var logErrorRef = debugType.GetMethod("LogError", new []{ typeof(object) });
                //
                // MethodInfo invokeCallbackMethod = typeof(EventSubscriber).GetMethod("InvokeCallback");
                //
                // ilgen.Emit(OpCodes.Ldarg_0);
                // ilgen.Emit(OpCodes.Call, invokeCallbackMethod);
                // ilgen.Emit(OpCodes.Ret); 
                //
                // _delegate = handler.CreateDelegate(tDelegate, this);
                
                Type tDelegate = eventInfo.EventHandlerType;

                // Add the EventSubscriber type as the first parameter type
                Type[] parameterTypes = GetDelegateParameterTypes(tDelegate);
                Type[] parameterTypesWithThis = new Type[parameterTypes.Length + 1];
                parameterTypesWithThis[0] = typeof(EventSubscriber);
                Array.Copy(parameterTypes, 0, parameterTypesWithThis, 1, parameterTypes.Length);

                DynamicMethod handler = new DynamicMethod("", null, parameterTypesWithThis);
                ILGenerator ilgen = handler.GetILGenerator();

                MethodInfo invokeCallbackMethod = typeof(EventSubscriber).GetMethod("InvokeCallback");

                // Load the EventSubscriber instance onto the stack
                ilgen.Emit(OpCodes.Ldarg_0);

                // Call the function
                ilgen.Emit(OpCodes.Call, invokeCallbackMethod);

                ilgen.Emit(OpCodes.Ret);

                // Create a delegate that takes the EventSubscriber instance as the first argument
                _delegate = handler.CreateDelegate(tDelegate, this);
            }
            // else if (_memberInfo is FieldInfo fieldInfo)
            // {
            //     _delegate = Delegate.CreateDelegate(fieldInfo.FieldType, this, nameof(_callback.Invoke));
            // }
        }

        #endregion
        
        private Type[] GetDelegateParameterTypes(Type d)
        {
            if (d.BaseType != typeof(MulticastDelegate))
                throw new ArgumentException("Not a delegate.", nameof(d));

            MethodInfo invoke = d.GetMethod("Invoke");
            if (invoke == null)
                throw new ArgumentException("Not a delegate.", nameof(d));

            ParameterInfo[] parameters = invoke.GetParameters();
            Type[] typeParameters = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                typeParameters[i] = parameters[i].ParameterType;
            }
            return typeParameters;
        }

        private Type GetDelegateReturnType(Type d)
        {
            if (d.BaseType != typeof(MulticastDelegate))
                throw new ArgumentException("Not a delegate.", nameof(d));

            MethodInfo invoke = d.GetMethod("Invoke");
            if (invoke == null)
                throw new ArgumentException("Not a delegate.", nameof(d));

            return invoke.ReturnType;
        }

        public void InvokeCallback()
        {
            _callback.Invoke();
            if (_unsubscribeExecutionTiming == EDecommissioning.Fire) Unsubscribe();
        }
    }

    [CustomEditor(typeof(EventSubscriber))]
    public class EventSubscriberEditor : OdinEditor
    {
        private EventSubscriber _target;
        private Type _targetType;
        
        private bool _isInitialized;


        protected override void OnEnable()
        {
            base.OnEnable();
            
            if (_isInitialized) return;

            _target = (EventSubscriber)target;
            _targetType = _target.GetType();
            
            _isInitialized = true;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (_targetType.GetField("_target") == null) return;
            
            // var eventFields = _target.target.GetType().GetEvents(EventSubscriber.BINDING_FLAGS);
            // var unityEventFields = _target.target.GetType().GetFields(EventSubscriber.BINDING_FLAGS).Where(x => x.FieldType.IsSubclassOf(typeof(UnityEventBase)));
            //
            //
            //
            //
            // var index = EditorGUILayout.Popup("Events", 0, eventFields.Select(x => x.Name).ToArray());
            // EditorGUILayout.Popup("Events", 0, unityEventFields.Select(x => x.Name).ToArray());
            //
            // serializedObject.FindProperty("_eventFieldName").stringValue = eventFields[index].Name;
            // serializedObject.ApplyModifiedProperties();
        }
    }
}
