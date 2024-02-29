using System;

namespace Project._Project.Scripts.StateMachine
{
    [Serializable]
    public abstract class BaseStateMachine : IDisposable
    {
        protected PCPlayerRefs playerRefs { get; private set; }
        

        public void Enter(PCPlayerRefs refs)
        {
            playerRefs = refs;
            
            OnEnter();
        }
        
        protected virtual void OnEnter() {}

        public virtual void Update() {}

        public void Exit()
        {
            OnExit();
            Dispose();
        }
        
        protected virtual void OnExit() {} 
        
        
        /// <summary>
        /// Check if we can change the actual state with the <paramref name="newStateMachine"/>
        /// </summary>
        /// <param name="newStateMachine">new player state to compare</param>
        /// <returns>
        /// <para><b>-true</b>: if the change is allowed</para>
        /// <para><b>-false</b>: if the change is not allowed</para>
        /// </returns>
        public abstract bool CanChangeStateTo(BaseStateMachine newStateMachine);
        
        public virtual void Dispose()
        {
            playerRefs = null;
        }
        
        public abstract override string ToString();
    }
}
