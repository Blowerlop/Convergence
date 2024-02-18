using UnityEngine;

namespace Project._Project.Scripts.StateMachine
{
    public abstract class BaseStateMachine
    {
        protected PlayerRefs playerRefs;
        
        /// <summary>
        /// Called on start state (not the same as the Unity event "Start")
        /// </summary>
        /// <param name="refs"></param>
        public virtual void StartState(PlayerRefs refs)
        {
            playerRefs = refs;
            Debug.Log($"<color=green>[{playerRefs.PlayerTransform.name}] Enter state {ToString()}</color> ");
        }
    
        /// <summary>
        /// Called with the Unity event "Update"
        /// </summary>
        public virtual void UpdateState() {}

        /// <summary>
        /// Called on state end
        /// </summary>
        public virtual void EndState()
        {
            Debug.Log($"<color=orange>[{playerRefs.PlayerTransform.name}] Exit state {ToString()}</color> ");
            playerRefs = null; // Release memory
        }
        
        /// <summary>
        /// Check if we can change the actual state with the <paramref name="newStateMachine"/>
        /// </summary>
        /// <param name="newStateMachine">new player state to compare</param>
        /// <returns>
        /// <para><b>-true</b>: if the change is allowed</para>
        /// <para><b>-false</b>: if the change is not allowed</para>
        /// </returns>
        public abstract bool CanChangeStateTo(BaseStateMachine newStateMachine);

        public abstract override string ToString();
    }
}
