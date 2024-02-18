using Project._Project.Scripts.StateMachine;

namespace Project._Project.Scripts.Player.State.PlayerState.Base.Idle
{
    public class IdleState : BaseStateMachine
    {
        public override bool CanChangeStateTo(BaseStateMachine newStateMachine)
        {
            return true;
        }

        public override string ToString()
        {
            return "Idle";
        }
    }
}
