using Project._Project.Scripts.StateMachine;

namespace Project._Project.Scripts.Player.States
{
    public class IdleState : BaseStateMachine
    {
        protected override void OnEnter()
        {
            playerRefs.Animator.SetBool(Constants.AnimatorsParam.Movement, false);
        }

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
