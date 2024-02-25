using Project._Project.Scripts.Player.State.PlayerState.Base.Idle;
using Project._Project.Scripts.StateMachine;

namespace Project._Project.Scripts.Player.State.PlayerState.Base
{
    public class PlayerStateMachineController : BaseStateMachineController
    {
        protected override BaseStateMachine defaultState { get; set; } = new IdleState();
    }
}
