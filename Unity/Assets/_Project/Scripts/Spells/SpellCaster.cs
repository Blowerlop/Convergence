using UnityEngine;

namespace Project._Project.Scripts.Spells
{
    public abstract class SpellCaster : MonoBehaviour
    {
        public bool IsChanneling { get; private set; }
        
        public virtual void StartChanneling()
        {
            if (IsChanneling) return;
            
            IsChanneling = true;
        }
        
        public virtual void StopChanneling()
        {
            if (!IsChanneling) return;
            
            IsChanneling = false;
        }
        
        protected virtual void Update()
        {
            if (!IsChanneling) return;
            
            EvaluateResults();
            UpdateChanneling();
        }

        /// <summary>
        /// Called every frame if this caster is channeling.
        /// Called after EvaluateResults().
        /// </summary>
        protected abstract void UpdateChanneling();

        /// <summary>
        /// Should evaluate current results based on user inputs and other factors. Then update _currentResults.
        /// Called before UpdateChanneling().
        /// </summary>
        protected abstract void EvaluateResults();

        /// <summary>
        /// Returns the current results of this caster.
        /// </summary>
        public abstract ICastResult GetResults();
    }
}