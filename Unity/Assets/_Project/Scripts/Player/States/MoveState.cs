using Project._Project.Scripts.StateMachine;
using UnityEngine;

namespace Project._Project.Scripts.Player.States
{
    public class MoveState : BaseStateMachine
    {
        protected override void OnEnter()
        {
            playerRefs.NavMeshAgent.isStopped = false;
            playerRefs.Animator.SetBool(Constants.AnimatorsParam.Movement, true);
        }

        protected override void OnExit()
        {
            var navMeshAgent = playerRefs.NavMeshAgent;
            
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
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
