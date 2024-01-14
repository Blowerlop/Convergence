using Unity.Netcode;
using UnityEngine;

namespace Project.Spells
{
    public class SpellManager : NetworkSingleton<SpellManager>
    {
        [SerializeField] private ScriptableObjectReferencesCache _scriptableObjectReferencesCache;
        
        public void TryCastSpell(int clientId, int spellIndex, ICastResult results)
        {
            if (!IsServer && !IsHost) return;
            
            UserInstance user = UserInstanceManager.instance.GetUserInstance(clientId);
            if (user == null)
            {
                Debug.LogError("Trying to cast a spell for an invalid user.");
                return;
            }

            PlayerRefs playerRefs = user.LinkedPlayer;
            if (playerRefs == null)
            {
                Debug.LogError("Trying to cast a spell for a user that have no player assigned.");
                return;
            }

            if (!SOCharacter.TryGetCharacter(_scriptableObjectReferencesCache, user.CharacterId, out var characterData))
            {
                Debug.LogError($"Trying to cast a spell for an invalid character {user.CharacterId}.");
                return;
            }

            if (!characterData.TryGetSpell(spellIndex, out var spell))
            {
                Debug.LogError($"Trying to cast an invalid spell : {spellIndex} of character {characterData.characterName}.");
                return;
            }
            
            PlayerPlatform platform = user.GetPlatform();
            CooldownController cooldownController = playerRefs.GetCooldownController(platform);
            
            if (cooldownController.IsInCooldown(spellIndex)) return;
            
            cooldownController.StartServerCooldown(spellIndex, spell.cooldown);
            
            Spell spellInstance = SpawnSpell(spell, results, playerRefs);
            spellInstance.Init(results);
        }

        private Spell SpawnSpell(SpellData spell, ICastResult results, PlayerRefs playerRefs)
        {
            Spell spellPrefab = spell.spellPrefab;
            
            (Vector3 pos, Quaternion rot) trans = spellPrefab.GetDefaultTransform(results, playerRefs);
            
            Spell spellInstance = Instantiate(spellPrefab, trans.pos, trans.rot);
            spellInstance.NetworkObject.Spawn();
            
            return spellInstance;
        }

        #region TryCast Interface

        //Since Netcode doesn't really handle polymorphism, we need to create a TryCast method for each channeling results
        
        [ServerRpc(RequireOwnership = false)]
        public void TryCastSpellServerRpc(int spellIndex, SingleVectorResults results, ServerRpcParams serverRpcParams = default)
        {
            TryCastSpell((int)serverRpcParams.Receive.SenderClientId, spellIndex, results);
        }

        #endregion

    }
}