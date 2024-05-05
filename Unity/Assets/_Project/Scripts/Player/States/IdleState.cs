using Project._Project.Scripts.StateMachine;

namespace Project._Project.Scripts.Player.States
{
    public class IdleState : BaseStateMachineBehaviour
    {
        protected override void OnEnter()
        {
            playerRefs.Animator.SetBool(Constants.AnimatorsParam.Movement, false);
        }

        public override bool CanChangeStateTo<T>()
        {
            return true;
        }

        public override string ToString()
        {
            return "Idle";
        }
    }
}
