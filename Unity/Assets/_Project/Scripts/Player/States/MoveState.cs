using Project._Project.Scripts.StateMachine;

namespace Project._Project.Scripts.Player.State
{
    public class MoveState : BaseStateMachine
    {
        public override bool CanChangeStateTo(BaseStateMachine newStateMachine)
        {
            return true;
        }

        public override string ToString()
        {
            return "Move";
        }
    }
}
