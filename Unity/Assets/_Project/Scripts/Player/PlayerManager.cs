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

            Vector3 pos = Random.insideUnitSphere * 4f;
            pos.y = 1;

            var obj = Instantiate(characterData.prefab, pos, Quaternion.identity);
            obj.GetComponent<NetworkObject>().SpawnWithOwnership((ulong)charTeam.pcPlayerOwnerClientId);

            obj.GetComponent<PlayerRefs>().ServerInit(teamId, characterData);
        }
    }
}