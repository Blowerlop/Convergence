using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project
{
    [System.Serializable]
    public class DataLogTeamInfo{
        public string PCPlayerName;
        public string PCPlayerCharacter; 
        public string MobilePlayerName;
    }
    [System.Serializable]
    public class DataLogMatch
    {
        public int winnerTeam;
        public List<DataLogTeamInfo> TeamInfoList = new List<DataLogTeamInfo>();

        public DataLogMatch(int teamIndex)
        {
            this.winnerTeam = teamIndex; 

        }
    }
}
