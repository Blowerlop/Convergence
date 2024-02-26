using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project._Project.Scripts.StateMachine
{
    public abstract class BaseStateMachineController : MonoBehaviour
    {
        protected abstract BaseStateMachine defaultState { get; set; }
        [ShowInInspector] public BaseStateMachine currentState { get; private set; }
        
        private PlayerRefs _playerRefs;

        public event Action<BaseStateMachine> OnStateEnter;
        public event Action<BaseStateMachine> OnStateExit;
        
        
        private void Awake()
        {
            _playerRefs = GetComponent<PlayerRefs>();
        }
        
        private void Start()
        {
            currentState = defaultState;
            currentState.StartState(_playerRefs);
        }

        public virtual void Update()
        {
            currentState.UpdateState();
        }

        /// <summary>
        /// <para>Switch this state with another: <paramref name="newStateMachine"/> </para>
        /// /!\ do a <see cref="CanChangeStateTo"/> before
        /// </summary>
        /// <param name="newStateMachine">new player state</param>
        public virtual void ChangeState(BaseStateMachine newStateMachine)
        {
            BaseStateMachine previousState = currentState;
            OnStateExit?.Invoke(previousState);
            previousState.EndState();
            
            Debug.Log($"<color=#00D8FF>[{name}]</color> <color=orange>{previousState}</color> => <color=#00D8FF>{newStateMachine}</color>");
            
            currentState = newStateMachine;
            currentState.StartState(_playerRefs);
            OnStateEnter?.Invoke(currentState);
        }

        public virtual bool CanChangeStateTo(BaseStateMachine newStateMachine)
        {
            return currentState.CanChangeStateTo(newStateMachine);
        }
    }
}
