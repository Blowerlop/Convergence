using TMPro;
using UnityEngine;

namespace Project
{
    public class TeamHeaderItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI teamNameText;
        [SerializeField] private TextMeshProUGUI winCountText;
        
        private int _teamIndex;
        
        public void Init(TeamData teamData, int teamIndex)
        {
            if (!teamData.TryGetUserInstance(PlayerPlatform.Pc, out var pcUser))
            {
                Debug.LogError($"Team {teamIndex} has no PC player!");
                return;
            }
            
            teamNameText.text = $"Team {teamIndex + 1} " + (pcUser.IsOwner ? "(You)" : "");
            OnWinCountChanged(0, pcUser.WinCount.Value);
            
            pcUser.WinCount.OnValueChanged += OnWinCountChanged;
        }

        private void OnWinCountChanged(int _, int newValue)
        {
            winCountText.text = $"{newValue}";
        }
    }
}