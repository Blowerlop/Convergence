using System;
using System.Collections.Generic;
using System.Linq;
using Project._Project.Scripts.Player.States;
using Project.Extensions;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Project._Project.Scripts.StateMachine
{
    public class StateMachineController : NetworkBehaviour
    {
        [SerializeReference] protected BaseStateMachineBehaviour defaultState;
        [field: SerializeReference, Sirenix.OdinInspector.ReadOnly] public BaseStateMachineBehaviour currentState { get; private set; }
        [SerializeReference] protected List<BaseStateMachineBehaviour> _states;
        private readonly Dictionary<Type, BaseStateMachineBehaviour> _statesCache = new Dictionary<Type, BaseStateMachineBehaviour>();
        
        private PCPlayerRefs _playerRefs;

        public event Action<BaseStateMachineBehaviour> OnStateEnter;
        public event Action<BaseStateMachineBehaviour> OnStateExit;

        private readonly GRPC_NetworkVariable<FixedString32Bytes> _currentStateType = new("currentStateType");

        private void Start()
        {
            _currentStateType.Initialize();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _playerRefs = GetComponent<PCPlayerRefs>();

                PopulateCache();
                
                currentState = defaultState;
                currentState.Enter(_playerRefs);
                Debug.Log($"<color=#00D8FF>[{_playerRefs.PlayerTransform.name}]</color> <color=orange>Entered default state '{currentState}'</color>");
            }
            else if (this.IsClientOnly())
            {
                enabled = false;
                
                _currentStateType.OnValueChanged += OnCurrentStateTypeChanged;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (!this.IsClientOnly()) return;
                
            _currentStateType.OnValueChanged -= OnCurrentStateTypeChanged;
        }

        private void Update()
        {
            currentState.Update();
        }

        /// <summary>
        /// Change to the same state is prohibited
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [Server]
        public void ChangeStateTo<T>() where T : BaseStateMachineBehaviour
        {
            ChangeStateTo(GetState<T>());
        }
        
        /// <summary>
        /// Change to the same state is prohibited
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [Server]
        public void ChangeStateTo(BaseStateMachineBehaviour state)
        {
            if (currentState.Equals(state))
            {
                Debug.Log("Equality");
                return;
            }
            
            BaseStateMachineBehaviour previousState = currentState;
            currentState = state;
            
            OnStateExit?.Invoke(previousState);
            previousState.Exit();
            
            Debug.Log($"<color=#00D8FF>[{_playerRefs.PlayerTransform.name}]</color> <color=orange>{previousState}</color> => <color=#00D8FF>{currentState}</color>");
            
            currentState.Enter(_playerRefs);
            OnStateEnter?.Invoke(currentState);
            
            _currentStateType.Value = currentState.GetType().Name;
        }

        public bool CanChangeStateTo<T>() where T : BaseStateMachineBehaviour
        {
            return currentState.CanChangeStateTo<T>();
        }
        
        [Server]
        public bool TryChangeStateTo<T>() where T : BaseStateMachineBehaviour
        {
            if (CanChangeStateTo<T>())
            {
                ChangeStateTo<T>();
                return true;
            }

            return false;
        }
        
        private T GetState<T>() where T : BaseStateMachineBehaviour
        {
            if (_statesCache.TryGetValue(typeof(T), out BaseStateMachineBehaviour state))
            {
                return (T)state;
            }

            Debug.LogWarning($"State {typeof(T).Name} not cached, creating a new instance. Make sur to cache it for better memory usage.");
            T instance = Activator.CreateInstance<T>();
            _statesCache.Add(typeof(T), instance);
            return instance;
        }

        private void PopulateCache()
        {
            _states.Add(defaultState);
            _states = _states.DistinctBy(state => state.GetType()).ToList();

            _states.ForEach(state => _statesCache.Add(state.GetType(), state));
            // _states = null;
        }
        
        #region ClientSide
        
        /// <summary>
        /// Update current state on client using type. Not activating any logic on clients.
        /// Just letting them know if they can switch state or not.
        /// </summary>
        /// <param name="_"></param>
        /// <param name="newType"></param>
        private void OnCurrentStateTypeChanged(FixedString32Bytes _, FixedString32Bytes newType)
        {
            // TODO: Find something better to get an instance, maybe an index or idk
            var fullName = typeof(AttackState).Namespace + "." + newType;
            var type = Type.GetType(fullName);
            
            if (type == null)
            {
                Debug.LogError($"Type {newType} not found.");
                return;
            }
            
            currentState = Activator.CreateInstance(type) as BaseStateMachineBehaviour;
            
            if(currentState == null)
            {
                Debug.LogError($"Type {newType} is not a BaseStateMachineBehaviour.");
                return;
            }
            
            //Debug.Log("Changed State " + currentState.GetType().Name);
        }
        
        #endregion
    }
}
