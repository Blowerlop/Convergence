using Project._Project.Scripts.StateMachine;
using UnityEngine;

namespace Project._Project.Scripts.Player.States
{
    public class CastingState : BaseStateMachine
    {
        protected override void OnEnter()
        {
            playerRefs.Animator.SetBool(Constants.AnimatorsParam.Channeling, true);
        }
        
        protected override void OnExit()
        {
            playerRefs.Animator.SetBool(Constants.AnimatorsParam.Channeling, false);
        }
        
        public override bool CanChangeStateTo(BaseStateMachine newStateMachine)
        {
            return newStateMachine is IdleState or MoveState;
        }

        public override string ToString()
        {
            return "Casting";
        }
    }
}
