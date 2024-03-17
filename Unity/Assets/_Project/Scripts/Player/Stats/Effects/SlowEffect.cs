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

            MoveSpeedStat stat = pcPlayer.Entity.Stats.Get<MoveSpeedStat>();
            
            var slowedValue = stat.Slow(SlowAmount);

            player.StartCoroutine(Utilities.WaitForSecondsAndDoActionCoroutine(Duration, 
                    () => { stat.value += slowedValue; }));
            
            return false;
        }
    }
}