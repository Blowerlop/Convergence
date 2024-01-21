using System;
using Project._Project.Scripts.Spells;
using Unity.Netcode;
using UnityEngine;

namespace Project.Spells
{
    public class SpellManager : NetworkSingleton<SpellManager>
    {
        [SerializeField] private SOScriptableObjectReferencesCache _soScriptableObjectReferencesCache;
        
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

            if (!TryGetSpellData(user, spellIndex, out var spell)) return;
            
            PlayerPlatform platform = user.GetPlatform();
            
            ChannelingController channelingController = playerRefs.GetChannelingController(platform);
            if (channelingController.IsChanneling) return;
            
            CooldownController cooldownController = playerRefs.GetCooldownController(platform);
            if (cooldownController.IsInCooldown(spellIndex)) return;
            
            cooldownController.StartServerCooldown(spellIndex, spell.cooldown);
            
            channelingController.StartServerChanneling(spell.channelingTime,
                () => OnChannelingEnded(spell, results, playerRefs));
        }

        private void OnChannelingEnded(SpellData spell, ICastResult results, PlayerRefs playerRefs)
        {
            Spell spellInstance = SpawnSpell(spell, results, playerRefs);
            spellInstance.Init(results);
        }

        private bool TryGetSpellData(UserInstance user, int spellIndex, out SpellData spell)
        {
            spell = null;
            
            switch (user.GetPlatform())
            {
                case PlayerPlatform.Pc:
                    if (!SOCharacter.TryGetCharacter(_soScriptableObjectReferencesCache, user.CharacterId,
                            out var characterData))
                    {
                        Debug.LogError($"Trying to cast a spell for an invalid character {user.CharacterId}.");
                        return false;
                    }
                    if (!characterData.TryGetSpell(spellIndex, out spell))
                    {
                        Debug.LogError($"Trying to cast an invalid spell : {spellIndex} of character " +
                                       $"{characterData.characterName}.");
                        return false;
                    }
                    break;
                case PlayerPlatform.Mobile:
                    spell = SpellData.GetSpell(_soScriptableObjectReferencesCache, spellIndex);

                    if (spell == null)
                    {
                        Debug.LogError($"Trying to cast an invalid spell : {spellIndex} of mobile player " +
                                       $"{user.ClientId}.");
                        return false;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
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