using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class CharacterRefs : NetworkBehaviour
    {
        private GRPC_NetworkVariable<int> assignedTeam = new GRPC_NetworkVariable<int>("AssignedTeam", value: -1);

        // Is done before ServerInit on server
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            assignedTeam.Initialize();

            Debug.LogError(assignedTeam.Value);
            
            if (!IsClient) return;

            assignedTeam.OnValueChanged += OnTeamChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            if (!IsClient) return;

            assignedTeam.OnValueChanged -= OnTeamChanged;
        }

        public void ServerInit(int team)
        {
            if (!IsServer && !IsHost) return;

            Debug.LogError($"Set to {team}");
            assignedTeam.Value = team;
        }

        private void OnTeamChanged(int oldValue, int newValue)
        {
            Debug.LogError("On team changed");
            
            var oldTeam = UserInstanceManager.instance.GetTeam(oldValue);
            var newTeam = UserInstanceManager.instance.GetTeam(newValue);

            if (!oldTeam.IsEmpty)
            {
                // Do not unlink if another character has already been linked to old team
                if(oldTeam.MobileUser.LinkedCharacter == this) oldTeam.MobileUser.UnlinkCharacter();
                if(oldTeam.PcUser.LinkedCharacter == this) newTeam.PcUser.UnlinkCharacter();
            }
            
            if (newTeam.PcUser) newTeam.PcUser.LinkCharacter(this);
            if (newTeam.MobileUser) newTeam.MobileUser.LinkCharacter(this);
        }
    }
}