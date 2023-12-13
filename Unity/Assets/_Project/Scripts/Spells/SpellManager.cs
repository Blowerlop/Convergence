using Project.Spells.Casters;
using Unity.Netcode;
using UnityEngine;

namespace Project.Spells
{
    public class SpellManager : NetworkSingleton<SpellManager>
    {
        [SerializeField] private SpellCastController spellCast;
        [SerializeField] private CooldownController cooldown;
        [SerializeField] private SpellDatasList spells;
        
        private void TryCastSpell(int clientId, int spellIndex, IChannelingResult results)
        {
            // TODO: Get right CooldownController from clientId
            if (cooldown.IsInCooldown(spellIndex)) return;
            
            // TODO: Get right spell from client character
            var spell = spellCast.GetSpellAtIndex(spellIndex);
            
            cooldown.StartServerCooldown(spellIndex, spell.cooldown);
            
            Spell spellPrefab = spell.spellPrefab;
            
            (Vector3 pos, Quaternion rot) trans = spellPrefab.GetDefaultTransform(results);
            
            Spell spellInstance = Instantiate(spellPrefab, trans.pos, trans.rot);
            spellInstance.NetworkObject.Spawn();
            
            spellInstance.Init(results);
        }

        #region TryCast Interface

        //Since Netcode doesn't really handle polymorphism, we need to create a TryCast method for each channeling results
        
        [ServerRpc(RequireOwnership = false)]
        public void TryCastSpellServerRpc(int spellIndex, DefaultSkillShotResults results, ServerRpcParams serverRpcParams = default)
        {
            TryCastSpell((int)serverRpcParams.Receive.SenderClientId, spellIndex, results);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void TryCastSpellServerRpc(int spellIndex, DefaultZoneResults results, ServerRpcParams serverRpcParams = default)
        {
            TryCastSpell((int)serverRpcParams.Receive.SenderClientId, spellIndex, results);
        }

        #endregion

    }
}