using Project._Project.Scripts.StateMachine;
using UnityEngine;

namespace Project._Project.Scripts.Player.State
{
    public class MoveState : BaseStateMachine
    {
        public override void StartState(PlayerRefs refs)
        {
            base.StartState(refs);
            
            playerRefs.StateMachine.GetComponentInChildren<Animator>().SetBool(Constants.AnimatorsParam.Movement, true);
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
