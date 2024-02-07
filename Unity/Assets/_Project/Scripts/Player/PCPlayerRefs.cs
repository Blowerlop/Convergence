using Project.Spells.Casters;
using UnityEngine;

namespace Project
{
    public class PCPlayerRefs : PlayerRefs
    {
        [SerializeField] private SpellCastController spellCastController;

        protected override void OnOwnerChanged(int oldId, int newId)
        {
            base.OnOwnerChanged(oldId, newId);
            
            spellCastController.Init(this);
        }

        protected override void OnTeamChanged(int oldValue, int newValue)
        {
            base.OnTeamChanged(oldValue, newValue);
            
            // Maybe change color or something idk
        }
    }
}