using Project._Project.Scripts.StateMachine;
using UnityEngine;

namespace Project._Project.Scripts.Player.States
{
    public class CastingState : BaseStateMachineBehaviour
    {
        protected override void OnEnter()
        {
            playerRefs.Animator.SetBool(Constants.AnimatorsParam.Channeling, true);
        }
        
        protected override void OnExit()
        {
            playerRefs.Animator.SetBool(Constants.AnimatorsParam.Channeling, false);
        }
        
        public override bool CanChangeStateTo<T>()
        {
            return typeof(T) == typeof(IdleState);
        }

        public override string ToString()
        {
            return "Casting";
        }
    }
}
