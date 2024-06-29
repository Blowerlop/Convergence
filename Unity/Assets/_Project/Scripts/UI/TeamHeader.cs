using System.Linq;
using UnityEngine;

namespace Project
{
    public class TeamHeader : MonoBehaviour
    {
        [SerializeField] private TeamHeaderItem prefab;
        [SerializeField] private Transform prefabParent;
        
        private void Start()
        {
            foreach(var user in UserInstanceManager.instance.GetUsersInstance())
            {
                if (user.IsMobile) continue;
                var teamIndex = user.Team;

                var mobileUser = UserInstanceManager.instance.GetUsersInstance()
                    .FirstOrDefault(u => u.IsMobile && u.Team == teamIndex);
                
                var teamHeader = Instantiate(prefab, prefabParent);
                teamHeader.Init(user, mobileUser, teamIndex);
            }
        }
    }
}
