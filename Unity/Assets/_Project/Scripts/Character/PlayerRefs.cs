using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class PlayerRefs : NetworkBehaviour
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
            var oldTeamResult = TeamManager.instance.TryGetTeam(oldValue, out var oldTeam);
            var newTeamResult = TeamManager.instance.TryGetTeam(newValue, out var newTeam);

            if (oldTeamResult)
            {
                UserInstance oldMobile = oldTeam.GetUserInstance(PlayerPlatform.Mobile);
                UserInstance oldPc = oldTeam.GetUserInstance(PlayerPlatform.Pc);
                
                // Do not unlink if another character has already been linked to old team
                if(oldMobile != null && oldMobile.LinkedPlayer == this) oldMobile.UnlinkCharacter();
                if(oldPc != null && oldPc.LinkedPlayer == this) oldPc.UnlinkCharacter();
            }

            if (newTeamResult)
            {
                UserInstance newMobile = newTeam.GetUserInstance(PlayerPlatform.Mobile);
                UserInstance newPc = newTeam.GetUserInstance(PlayerPlatform.Pc);
            
                if (newPc) newPc.LinkCharacter(this);
                if (newMobile) newMobile.LinkCharacter(this);
            }
        }
    }
}