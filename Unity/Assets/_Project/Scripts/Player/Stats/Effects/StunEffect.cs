using Project._Project.Scripts;
using Project._Project.Scripts.Player.States;
using UnityEngine;

namespace Project.Effects
{
    public class StunEffect : Effect 
    {
        public float Duration;

        public override EffectType Type => EffectType.Bad;

        private PCPlayerRefs _affectedPlayer;
        private Coroutine _appliedCoroutine;
        
        [Server]
        protected override bool TryApply_Internal(IEffectable effectable, PlayerRefs applier, Vector3 applyPosition)
        {
            var entity = effectable.AffectedEntity;
            
            if(!entity.TryGetComponent(out _affectedPlayer))
            {
                Debug.LogWarning(
                    $"Can't apply StunEffect on Entity {entity.data.name} because it doesn't have a PCPlayerRefs");
                return false;
            }

            if (!_affectedPlayer.StateMachine.CanChangeStateTo<StunState>())
            {
                return false;
            }
            
            _affectedPlayer.StateMachine.ChangeStateTo<StunState>();
            
            AffectedEffectable.AffectedEntity.StartCoroutine(
                Utilities.WaitForSecondsAndDoActionCoroutine(Duration, RemoveStun));
            
            return true;
        }

        public override void KillEffect()
        {
            RemoveStun();
            AffectedEffectable.AffectedEntity.StopCoroutine(_appliedCoroutine);
        }

        private void RemoveStun()
        {
            _affectedPlayer.StateMachine.ChangeStateTo<IdleState>();
        }
        
        public override Effect GetInstance()
        {
            return new StunEffect()
            {
                Duration = Duration
            };
        }

        public override float GetEffectDuration()
        {
            return Duration;
        }
    }
}