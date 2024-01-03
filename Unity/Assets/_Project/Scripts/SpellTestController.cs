using Sirenix.OdinInspector;
using UnityEngine;

namespace Project._Project.Scripts
{
    public class SpellTestController : MonoBehaviour
    {
        [Button("Set Character Data")]
        public void SetCharacter(int clientId, SOCharacter data)
        {
            UserInstanceManager.instance.GetUserInstance(clientId).ServerSetCharacter(data.id);
        }
        
        [Button("Set Team")]
        public void SetTeam(int clientId, int teamId)
        {
            TeamManager.instance.TrySetTeam(clientId, teamId, PlayerPlatform.Pc);
        }
        
        [Button("Spawn Player")]
        public void SpawnPlayer(SOCharacter data, int team)
        {
            PlayerManager.instance.SpawnPlayer(team, data);
        }
    }
}