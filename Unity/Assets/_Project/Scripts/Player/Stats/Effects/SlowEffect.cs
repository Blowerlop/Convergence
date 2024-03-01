namespace Project.Effects
{
    public class SlowEffect : Effect 
    {
        public int SlowAmount;
        public float Duration;
        
        [Server]
        public override bool TryApply(PlayerRefs player)
        {
            if(player is not PCPlayerRefs pcPlayer)
                return false;
            
            var slowedValue = pcPlayer.Entity.Stats.moveSpeed.Slow(SlowAmount);
            
            player.StartCoroutine(Utilities.WaitForSecondsAndDoActionCoroutine(Duration, () =>
            {
                pcPlayer.Entity.Stats.moveSpeed.Value += slowedValue;
            }));
            
            return false;
        }
    }
}