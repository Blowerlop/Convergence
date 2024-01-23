#if UNITY_EDITOR
using Project._Project.Scripts.Utilities;
using Sirenix.OdinInspector;
using Unity.Netcode;

namespace Project
{
    public class ClientCompatibility : NetworkBehaviour
    {
        [ShowInInspector, ReadOnly] private string _gitCommitHash;
        
        
        private void Start()
        {
            _gitCommitHash = GetGitCommitHash();
        }

        public override void OnNetworkSpawn()
        {
            if (IsClient == false) return;
            
            CompareCommitHashWithServer_ServerRpc(NetworkManager.Singleton.LocalClientId, _gitCommitHash);
        }

        
        [ServerRpc(RequireOwnership = false)]
        private void CompareCommitHashWithServer_ServerRpc(ulong clientId, string gitCommitHash)
        {
            OnCommitHashCompared(clientId, _gitCommitHash == gitCommitHash);
        }

        [Server]
        private void OnCommitHashCompared(ulong clientId, bool result)
        {
            if (result)
            {
                NetworkManager.Singleton.DisconnectClient(clientId, "Last git commit hash different from server");
            }
        }
        
        private string GetGitCommitHash()
        {
            string commitHash = GitUtilities.RetrieveCurrentCommitShorthash();
            return commitHash;
        }
    }
}
#endif