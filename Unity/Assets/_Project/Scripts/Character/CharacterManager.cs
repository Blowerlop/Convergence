using System;
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
            SpawnPlayer(team, data);
        }
        
        public void SpawnPlayer(int teamId, SOCharacter characterData)
        {
            var team = UserInstanceManager.instance.GetTeam(teamId);
            
            if (team.PcUser == null)
            {
                Debug.LogError("Can't spawn player for a team that have no PCUser");
                return;
            }
            
            var obj = Instantiate(characterData.prefab);
            obj.GetComponent<NetworkObject>().SpawnWithOwnership(team.PcUser.OwnerClientId);

            obj.GetComponent<CharacterRefs>().ServerInit(teamId);
        }
    }
}