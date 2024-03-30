using Project._Project.Scripts;
using Project._Project.Scripts.Player.States;
using UnityEngine;

namespace Project.Effects
{
    public class StunEffect : Effect 
    {
        public float Duration;
        
        [Server]
        public override bool TryApply(Entity entity)
        {
            if(!entity.TryGetComponent(out PCPlayerRefs pcPlayerRefs))
            {
                Debug.LogWarning(
                    $"Can't apply StunEffect on Entity {entity.data.name} because it doesn't have a PCPlayerRefs");
                return false;
            }

            if (!pcPlayerRefs.StateMachine.CanChangeStateTo<StunState>())
            {
                return false;
            }
            
            pcPlayerRefs.StateMachine.ChangeStateTo<StunState>();
            
            entity.StartCoroutine(Utilities.WaitForSecondsAndDoActionCoroutine(Duration, 
                () => { pcPlayerRefs.StateMachine.ChangeStateTo<IdleState>(); }));
            
            return true;
        }
    }
}