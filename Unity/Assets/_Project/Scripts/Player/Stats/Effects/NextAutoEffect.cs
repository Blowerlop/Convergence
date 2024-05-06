using System.Collections.Generic;
using UnityEngine;

namespace Project.Effects
{
    public class NextAutoEffect : Effect
    {
        public override EffectType Type => EffectType.Neutral;

        [SerializeReference, SerializeField] private List<Effect> _effectsOnAuto = new();

        protected override bool TryApply_Internal(IEffectable effectable)
        {
            AddToEffectable();
            return true;
        }

        public override void KillEffect()
        {
            RemoveFromEffectable();
        }

        public bool TryApplyChildEffects(IEffectable effectable)
        {
            int appliedEffects = 0;
            
            foreach (var effect in _effectsOnAuto)
            {
                if (effect.TryApply(effectable))
                    appliedEffects++;
            }

            return appliedEffects > 0;
        }

        public override Effect GetInstance()
        {
            return new NextAutoEffect() { _effectsOnAuto = _effectsOnAuto };
        }
    }
}