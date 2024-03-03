using System.Linq;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Project.Spells
{
    public abstract class Spell : NetworkBehaviour
    {
        protected PlayerRefs Caster { get; private set; }
        protected int CasterTeamIndex => Caster.TeamIndex;
            
        [field: SerializeField, ReadOnly] public SpellData Data { get; set; }
        
        // Called by Server
        // Used to set field that are common for every spell before calling overriden Init
        [Server]
        public void Init(ICastResult castResult, PlayerRefs player)
        {
            Caster = player;
            
            Init(castResult);
        }
        
        protected abstract void Init(ICastResult castResult);

        public abstract (Vector3, Quaternion) GetDefaultTransform(ICastResult castResult, PlayerRefs player);
        
        public abstract Vector3 GetDirection(ICastResult castResult, PlayerRefs player);

        public virtual bool TryApplyEffects(PlayerRefs player)
        {
            int appliedEffects = Data.effects.Count(effect => effect.TryApply(player));
            return appliedEffects > 0;
        }
    }
}