using UnityEngine;

namespace Project
{
    public class TeamHeader : MonoBehaviour
    {
        [SerializeField] private TeamHeaderItem prefab;
        [SerializeField] private Transform prefabParent;
        
        private void Start()
        {
            var teams = TeamManager.instance.GetTeamsData();

            for (var i = 0; i < teams.Length; i++)
            {
                var team = teams[i];
                if (!team.HasPC) continue;
                
                var temp = i;
                
                var teamHeader = Instantiate(prefab, prefabParent);
                teamHeader.Init(team, temp);
            }
        }
    }
}
