using Project._Project.Scripts;
using Sirenix.OdinInspector;

namespace Project.Effects
{
    public class ShieldEffect : Effect 
    {
        public int ShieldAmount;
        public bool HasDuration;
        [ShowIf(nameof(HasDuration))] public float Duration;
        
        [Server]
        public override bool TryApply(Entity entity)
        {
            int shieldId = entity.Shield(ShieldAmount);

            if (!HasDuration) return true;
            
            entity.StartCoroutine(Utilities.WaitForSecondsAndDoActionCoroutine(Duration, 
                    () => { entity.UnShield(shieldId); }));
            
            return true;
        }
    }
}