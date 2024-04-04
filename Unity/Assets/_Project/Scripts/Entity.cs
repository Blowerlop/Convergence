using System.Collections.Generic;
using System;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Project._Project.Scripts
{
    public abstract class Entity : NetworkBehaviour, IHealable, IDamageable, IShieldable, IEffectable/*, ITargetable*/
    {
        [field: ShowInInspector, ReadOnly, ServerField] public SOEntity data { get; private set; }
        [SerializeField] protected PlayerStats _stats;
        public PlayerStats Stats => _stats;

        public virtual int TeamIndex => -1;

        public Entity AffectedEntity => this;
        public IList<Effect> AppliedEffects { get; } = new List<Effect>();
        
        private bool _isInit;
        private event Action _onEntityInit;
        public event Action onEntityInit
        {
            add
            {
                if (_isInit)
                {
                    value.Invoke();
                }
                else _onEntityInit += value;
            }
            remove => _onEntityInit -= value;
        }

        [Server]
        public void ServerInit(SOEntity entityData)
        {
            data = entityData;
            _stats.ServerInit(entityData);
            
            _onEntityInit?.Invoke();
            _onEntityInit = null;
            _isInit = true;
        }
        
        public void Heal(int modifier)
        {
            _stats.nHealthStat.Value += modifier;
        }

        public void MaxHeal()
        {
            _stats.nHealthStat.SetToMaxValue();
        }

        public void Damage(int modifier)
        {
            CheckForShieldDamage(ref modifier);
            
            _stats.nHealthStat.Value -= modifier;
        }

        private void CheckForShieldDamage(ref int modifier)
        {
            if (_stats.nShieldStat == null || !_stats.nShieldStat.HasShield) return;

            var lastValue = _stats.nShieldStat.Value;
            _stats.nShieldStat.Value -= modifier;
            
            modifier -= lastValue;
            if(modifier < 0) modifier = 0;
        }
        
        public bool CanDamage(int teamIndex)
        {
            return TeamIndex != teamIndex;
        }
        
        public int Shield(int modifier)
        {
            var shield = new ShieldData(modifier);
            
            _stats.nShieldStat.AddShield(shield);
            return shield.ID;
        }

        public void UnShield(int shieldId)
        {
            _stats.nShieldStat.RemoveShield(shieldId);
        }
        
        #region Effects
        
        [Server]
        public bool SrvTryApplyEffects(IList<Effect> effects) => 
            effects.Count(effect => effect.TryApply(this)) > 0;
        
        [Server]
        public void SrvAddEffect(Effect effect)
        {
            AppliedEffects.Add(effect);
        }
        
        [Server]
        public void SrvRemoveEffect(Effect effect)
        {
            AppliedEffects.Remove(effect);
        }
        
        [Server]
        public void SrvCleanse()
        {
            KillEffectsOfType(EffectType.Bad);
        }

        [Server]
        public void SrvDebuff()
        {
            KillEffectsOfType(EffectType.Good);
        }

        private void KillEffectsOfType(EffectType type)
        {
            var copy = new List<Effect>(AppliedEffects);
            foreach (var appliedEffect in copy.Where(effect => effect.Type == type))
            {
                appliedEffect.KillEffect();
            }
        }
        
        #endregion
    }
}