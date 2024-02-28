using Project._Project.Scripts.StateMachine;

namespace Project._Project.Scripts.Player.State.PlayerState.Base.Cast
{
    public class CastingState : BaseStateMachine
    {
        public override bool CanChangeStateTo(BaseStateMachine newStateMachine)
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return "Casting";
        }
    }
}
