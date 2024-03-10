using System;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Project._Project.Scripts.StateMachine
{
    public abstract class BaseStateMachineController : NetworkBehaviour
    {
        protected abstract BaseStateMachine defaultState { get; set; }
        [ShowInInspector, ReadOnly] public BaseStateMachine currentState { get; private set; }
        
        private PCPlayerRefs _playerRefs;

        public event Action<BaseStateMachine> OnStateEnter;
        public event Action<BaseStateMachine> OnStateExit;
        
        
        private void Awake()
        {
            _playerRefs = GetComponent<PCPlayerRefs>();
        }

        public override void OnNetworkSpawn()
        {
            currentState = defaultState;
            currentState.Enter(_playerRefs);
        }

        private void Update()
        {
            currentState.Update();
        }

        /// <summary>
        /// <para>Switch this state with another: <paramref name="to"/> </para>
        /// </summary>
        /// <param name="to">new player state</param>
        [Server]
        public void ChangeState(BaseStateMachine to)
        {
            if (to == currentState) return;
            
            BaseStateMachine previousState = currentState;
            currentState = to;
            
            OnStateExit?.Invoke(previousState);
            previousState.Exit();
            
            Debug.Log($"<color=#00D8FF>[{_playerRefs.PlayerTransform.name}]</color> <color=orange>{previousState}</color> => <color=#00D8FF>{to}</color>");
            
            currentState.Enter(_playerRefs);
            OnStateEnter?.Invoke(currentState);
        }

        public bool CanChangeStateTo(BaseStateMachine newStateMachine)
        {
            return currentState.CanChangeStateTo(newStateMachine);
        }
        
        public bool TryChangeState(BaseStateMachine newStateMachine)
        {
            if (CanChangeStateTo(newStateMachine))
            {
                ChangeState(newStateMachine);
                return true;
            }

            return false;
        }
    }
}
