namespace Project
{
    public class MobileStats : PlayerStats
    {
        // + attack speed / damage scale / other stats...
        
        //----
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        }
        
        public override void OnDestroy()
        {
            base.OnDestroy();
        }
        
        public override void ServerInit(SOCharacter character)
        {
            base.ServerInit(character);
        }
    }
}