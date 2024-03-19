using System;
using System.Collections.Generic;
using System.Linq;
using Project.Extensions;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Project._Project.Scripts.StateMachine
{
    public class StateMachineController : NetworkBehaviour
    {
        [SerializeReference] protected BaseStateMachineBehaviour defaultState;
        [field: SerializeReference, ReadOnly] public BaseStateMachineBehaviour currentState { get; private set; }
        [SerializeReference] protected List<BaseStateMachineBehaviour> _states;
        private readonly Dictionary<Type, BaseStateMachineBehaviour> _statesCache = new Dictionary<Type, BaseStateMachineBehaviour>();
        
        private PCPlayerRefs _playerRefs;

        public event Action<BaseStateMachineBehaviour> OnStateEnter;
        public event Action<BaseStateMachineBehaviour> OnStateExit;


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
            }
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
    }
}
