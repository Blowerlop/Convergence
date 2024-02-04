using Project.Spells.Casters;
using UnityEngine;

namespace Project
{
    public class PCPlayerRefs : PlayerRefs
    {
        [SerializeField] private SpellCastController spellCastController;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            spellCastController.Init(this);
        }

        protected override void OnTeamChanged(int oldValue, int newValue)
        {
            base.OnTeamChanged(oldValue, newValue);
            
            if (!UserInstanceManager.instance.TryGetUserInstance(OwnerId, out var user))
            {
                Debug.LogError($"OnTeamChanged > Can't find UserInstance for PlayerRefs with OwnerId {OwnerId}!");
                return;
            }
            
            // Maybe change color or something idk
        }
    }
}