using System;
using System.Linq;
using Project._Project.Scripts;
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
        
        [SerializeField] protected bool CanHitSelf;
        
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

        protected virtual bool TryApplyEffects(Entity entity)
        {
            if (!CanHitSelf && Caster.GetPC().Entity == entity)
                return false;
            
            int appliedEffects = Data.effects.Count(effect => effect.GetInstance().TryApply(entity, Caster.TeamIndex));
            return appliedEffects > 0;
        }

        protected virtual void KillSpell()
        {
            NetworkObject.Despawn();
        }
    }
}