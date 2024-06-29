using System.Linq;
using UnityEngine;

namespace Project
{
    public class MobilePlayerRefs : PlayerRefs
    {
        private PCPlayerRefs _pc;
        public override PCPlayerRefs GetPC() => _pc;

        protected override void OnTeamChanged(int _, int newValue)
        {
            base.OnTeamChanged(_, newValue);
            
            var pcUser = UserInstanceManager.instance.GetUsersInstance().FirstOrDefault(x => x.IsMobile == false && x.Team == newValue);
            if (pcUser == null)
            {
                Debug.LogError($"PC user for team {newValue} is null");
                return;
            }

            if (pcUser.LinkedPlayer == null)
            {
                Debug.LogError($"LinkedPlayer for PC user {pcUser.ClientId} is null");
                return;
            }
            
            playerTransform = pcUser.LinkedPlayer.PlayerTransform;
            shootTransform = pcUser.LinkedPlayer.ShootTransform;
            _pc = pcUser.LinkedPlayer as PCPlayerRefs;
            
            transform.parent = playerTransform;
            
            transform.localPosition = new Vector3(2.02f, 3.67f);
            transform.localRotation = Quaternion.identity;
        }
    }
}