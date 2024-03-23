using Project._Project.Scripts.StateMachine;
using UnityEngine;

namespace Project._Project.Scripts.Player.States
{
    public class StunState : BaseStateMachineBehaviour
    {
        protected override void OnEnter()
        {
            // Set silenced
            // Stop player + 0 ms
            //
        }
        
        protected override void OnExit()
        {
        }
        
        public override bool CanChangeStateTo<T>()
        {
            return false;
        }

        public override string ToString()
        {
            return "Stun";
        }
    }
}
