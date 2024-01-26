using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Project
{
    public class PlayerManager : NetworkSingleton<PlayerManager>
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsServer && !IsHost) return;
            
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoaded;
        }

        private void OnSceneLoaded(ulong clientid, string scenename, LoadSceneMode loadscenemode)
        {
            if (!UserInstanceManager.instance.TryGetUserInstance((int)clientid, out var user)) return;

            if (user.GetPlatform() == PlayerPlatform.Mobile) return;

            if (!TeamManager.instance.IsTeamIndexValid(user.Team))  return;

            if (!SOCharacter.TryGetCharacter(user.CharacterId, out var character))  return;
                
            SpawnPlayer(user.Team, character);
        }

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