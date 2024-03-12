using Project._Project.Scripts.StateMachine;

namespace Project._Project.Scripts.Player.States
{
    public class AttackState : BaseStateMachine
    {
        protected override void OnEnter()
        {
            playerRefs.Animator.ResetTrigger(Constants.AnimatorsParam.EndAttackInstant);
            playerRefs.Animator.SetTrigger(Constants.AnimatorsParam.Attack);
        }

        protected override void OnExit()
        {
            playerRefs.Animator.SetTrigger(Constants.AnimatorsParam.EndAttackInstant);
        }

        public override bool CanChangeStateTo(BaseStateMachine newStateMachine)
        {
            return newStateMachine is IdleState or MoveState;
        }

        public override string ToString()
        {
            return "Attack";
        }
    }
}