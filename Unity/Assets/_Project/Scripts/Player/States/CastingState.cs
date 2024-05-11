using System;
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
            Type type = typeof(T);
            return type == typeof(IdleState);
        }

        public override string ToString()
        {
            return "Casting";
        }
    }
}
