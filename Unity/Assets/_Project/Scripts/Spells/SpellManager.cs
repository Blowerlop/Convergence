using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Project.Spells
{
    public class SpellManager : NetworkBehaviour
    {
        #region Singleton
        public static SpellManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            else Instance = this;
        }
        #endregion
        
        [SerializeField] private SpellDatasList spells;
        
        private void TryCastSpell(int spellHash, IChannelingResult results)
        {
            var spell = spells.Get(spellHash);
            Spell spellPrefab = spell.spellPrefab;
            
            (Vector3 pos, Quaternion rot) trans = spellPrefab.GetDefaultTransform(results);
            
            Spell spellInstance = Instantiate(spellPrefab, trans.pos, trans.rot);
            spellInstance.NetworkObject.Spawn();
            
            spellInstance.Init(results);
        }

        #region TryCast Interface

        //Since Netcode doesn't really handle polymorphism, we need to create a TryCast method for each channeling results
        
        [ServerRpc(RequireOwnership = false)]
        public void TryCastSpellServerRpc(int spellHash, DefaultSkillShotResults results)
        {
            TryCastSpell(spellHash, results);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void TryCastSpellServerRpc(int spellHash, DefaultZoneResults results)
        {
            TryCastSpell(spellHash, results);
        }

        #endregion

    }
}