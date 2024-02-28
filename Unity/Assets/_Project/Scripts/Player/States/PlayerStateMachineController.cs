using Project._Project.Scripts.StateMachine;

namespace Project._Project.Scripts.Player.States
{
    public class PlayerStateMachineController : BaseStateMachineController
    {
        protected override BaseStateMachine defaultState { get; set; } = new IdleState();
        
        public readonly IdleState idleState = new IdleState();
        public readonly MoveState moveState = new MoveState();
        public readonly DeadState deadState = new DeadState();
    }
}
