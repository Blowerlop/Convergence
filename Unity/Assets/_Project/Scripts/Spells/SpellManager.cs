using System;
using Project._Project.Scripts.Spells;
using Unity.Netcode;
using UnityEngine;

namespace Project.Spells
{
    public class SpellManager : NetworkSingleton<SpellManager>
    {
        public static event Action<PlayerRefs, Vector3> OnChannelingStarted;
        
        [Server]
        public void TryCastSpell(int clientId, int spellIndex, ICastResult results)
        {
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
            
            ChannelingController channelingController = playerRefs.Channeling;
            if (channelingController.IsChanneling) return;
            
            CooldownController cooldownController = playerRefs.Cooldowns;
            if (cooldownController.IsInCooldown(spellIndex)) return;
            
            cooldownController.StartServerCooldown(spellIndex, spell.cooldown);
            
            channelingController.StartServerChanneling(spell.channelingTime,
                () => OnChannelingEnded(spell, results, playerRefs));
            
            OnChannelingStarted?.Invoke(playerRefs, spell.spellPrefab.GetDirection(results, playerRefs));
        }

        [Server]
        private void OnChannelingEnded(SpellData spell, ICastResult results, PlayerRefs playerRefs)
        {
            Spell spellInstance = SpawnSpell(spell, results, playerRefs);
            spellInstance.Init(results, playerRefs.TeamIndex);
        }

        [Server]
        private bool TryGetSpellData(UserInstance user, int spellIndex, out SpellData spell)
        {
            spell = null;
            
            switch (user.GetPlatform())
            {
                case PlayerPlatform.Pc:
                    if (!SOCharacter.TryGetCharacter(user.CharacterId,
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
                    var spellHash = user.GetMobileSpell(spellIndex);
                    
                    Debug.Log($"Trying to get spell {spellIndex} for mobile {user.ClientId} : {spellHash}");
                    
                    spell = SpellData.GetSpell(spellHash);

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
        
        [Server]
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
        
        [ServerRpc(RequireOwnership = false)]
        public void TryCastSpellServerRpc(int spellIndex, EmptyResults results, ServerRpcParams serverRpcParams = default)
        {
            TryCastSpell((int)serverRpcParams.Receive.SenderClientId, spellIndex, results);
        }

        #endregion

    }
}