using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class PlayerManager : NetworkSingleton<PlayerManager>
    {
        public void SpawnPlayer(int teamId, SOCharacter characterData)
        { 
            if (!TeamManager.instance.TryGetTeam(teamId, out var charTeam))
            {
                Debug.LogError("Trying to spawn a character for an invalid team.");
                return;
            }
            
            if (charTeam.pcPlayerOwnerClientId == int.MaxValue)
            {
                Debug.LogError("Can't spawn player for a team that have no PCUser");
                return;
            }
            
            var obj = Instantiate(characterData.prefab);
            obj.GetComponent<NetworkObject>().SpawnWithOwnership((ulong)charTeam.pcPlayerOwnerClientId);

            obj.GetComponent<PlayerRefs>().ServerInit(teamId);
        }
    }
}