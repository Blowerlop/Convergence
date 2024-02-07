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
            
            // Maybe change color or something idk
        }
    }
}