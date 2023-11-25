using UnityEngine;

namespace Project.Spells.Casters
{
    public abstract class SpellCaster : MonoBehaviour
    {
        public bool IsChanneling { get; private set; }

        protected Transform CasterTransform { get; private set; }
        
        public virtual void Init(Transform casterTransform, SpellData spell)
        {
            CasterTransform = casterTransform;
        }
        
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
        public abstract void EvaluateResults();

        /// <summary>
        /// Ask SpellManager to spawn the desired spell with caster current results.
        /// </summary>
        /// <param name="spellHash"></param>
        public abstract void TryCast(int spellHash);
    }
}