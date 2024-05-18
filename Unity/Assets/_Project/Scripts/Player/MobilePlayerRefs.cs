using UnityEngine;

namespace Project
{
    public class MobilePlayerRefs : PlayerRefs
    {
        public PCPlayerRefs PCPlayerRefs { get; private set; }
        
        protected override void OnTeamChanged(int _, int newValue)
        {
            base.OnTeamChanged(_, newValue);
            
            if(!TeamManager.instance.TryGetTeam(newValue, out var newTeam))
            {
                Debug.LogError($"New team with ID {newValue} is invalid");
                return;
            }

            if (!newTeam.TryGetUserInstance(PlayerPlatform.Pc, out var pcUser))
            {
                Debug.LogError($"New team with ID {newValue} has no PC player!");
                return;
            }

            if (pcUser.LinkedPlayer == null)
            {
                Debug.LogError($"LinkedPlayer for PC user {pcUser.ClientId} is null");
                return;
            }
            
            playerTransform = pcUser.LinkedPlayer.PlayerTransform;
            PCPlayerRefs = pcUser.LinkedPlayer as PCPlayerRefs;
        }
    }
}