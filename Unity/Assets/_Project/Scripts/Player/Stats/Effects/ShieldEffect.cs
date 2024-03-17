using Project._Project.Scripts;

namespace Project.Effects
{
    public class ShieldEffect : Effect 
    {
        public int ShieldAmount;
        public bool HasDuration;
        public float Duration;
        
        [Server]
        public override bool TryApply(PlayerRefs player)
        {
            if(player is not PCPlayerRefs pcPlayer)
                return false;

            Entity entity = pcPlayer.Entity;
            
            int shieldId = entity.Shield(ShieldAmount);

            if (!HasDuration) return true;
            
            player.StartCoroutine(Utilities.WaitForSecondsAndDoActionCoroutine(Duration, 
                    () => { entity.UnShield(shieldId); }));
            
            return true;
        }
    }
}