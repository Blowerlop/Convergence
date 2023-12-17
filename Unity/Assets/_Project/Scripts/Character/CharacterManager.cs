using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class CharacterManager : NetworkSingleton<CharacterManager>
    {
        [SerializeField] private SOCharacter data;
        [SerializeField] private int team;
        
        [Button("Debug SpawnPlayer")]
        public void Btn()
        {
            SpawnCharacter(team, data);
        }
        
        public void SpawnCharacter(int teamId, SOCharacter characterData)
        {
            var result = TeamManager.instance.TryGetTeam(teamId, out var charTeam);

            if (!result)
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

            obj.GetComponent<CharacterRefs>().ServerInit(teamId);
        }
    }
}