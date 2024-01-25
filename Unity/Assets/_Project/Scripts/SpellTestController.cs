using Sirenix.OdinInspector;
using UnityEngine;

namespace Project._Project.Scripts
{
    public class SpellTestController : MonoBehaviour
    {
        [Button("Set Character Data")]
        public void SetCharacter(int clientId, SOCharacter data)
        {
            UserInstanceManager.instance.GetUserInstance(clientId).SetCharacter(data.id);
        }
        
        [Button("Set Mobile Spell")]
        public void SetCharacter(int clientId, int spellHash, int slot)
        {
            UserInstanceManager.instance.GetUserInstance(clientId).SetMobileSpell(slot, spellHash);
        }
        
        [Button("Set Team")]
        public void SetTeam(int clientId, int teamId, PlayerPlatform platform)
        {
            TeamManager.instance.TrySetTeam(clientId, teamId, platform);
        }
        
        [Button("Spawn Player")]
        public void SpawnPlayer(SOCharacter data, int team)
        {
            PlayerManager.instance.SpawnPlayer(team, data);
        }
    }
}