using Project._Project.Scripts;
using UnityEngine;

namespace Project.Effects
{
    public class SlowEffect : Effect 
    {
        public int SlowAmount;
        public float Duration;
        
        [Server]
        public override bool TryApply(Entity entity)
        {
            if (!entity.Stats.TryGet(out MoveSpeedStat stat))
            {
                Debug.LogWarning(
                    $"Can't apply SlowEffect on Entity {entity.data.name} because it doesn't have a MoveSpeedStat");
                return false;
            }
            
            var slowedValue = stat.Slow(SlowAmount);

            entity.StartCoroutine(Utilities.WaitForSecondsAndDoActionCoroutine(Duration, 
                    () => { stat.value += slowedValue; }));
            
            return false;
        }
    }
}