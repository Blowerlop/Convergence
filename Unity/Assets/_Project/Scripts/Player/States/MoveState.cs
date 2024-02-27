using Project._Project.Scripts.StateMachine;

namespace Project._Project.Scripts.Player.States
{
    public class MoveState : BaseStateMachine
    {
        protected override void OnEnter()
        {
            playerRefs.Animator.SetBool(Constants.AnimatorsParam.Movement, true);
        }

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
