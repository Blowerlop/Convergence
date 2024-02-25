using Project._Project.Scripts.Player.State;
using Project._Project.Scripts.Player.State.PlayerState;
using Project._Project.Scripts.Player.State.PlayerState.Base.Idle;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project._Project.Scripts.StateMachine
{
    public abstract class BaseStateMachineController : MonoBehaviour
    {
        protected abstract BaseStateMachine defaultState { get; set; }
        [ShowInInspector] public BaseStateMachine currentState { get; private set; }

        public readonly IdleState idleState = new IdleState();
        public readonly MoveState moveState = new MoveState();
        

        private PlayerRefs _playerRefs;

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
            currentState.EndState();
            currentState = newStateMachine;
            currentState.StartState(_playerRefs);
        }

        public virtual bool CanChangeStateTo(BaseStateMachine newStateMachine)
        {
            return currentState.CanChangeStateTo(newStateMachine);
        }
    }
}
