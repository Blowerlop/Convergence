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
            teamNameText.text = $"Team {teamIndex + 1} " +
                "\n" + pcUser.PlayerName + (pcUser.IsOwner ? " (You)" : "") +
                "\n<i>" + (teamData.TryGetUserInstance(PlayerPlatform.Mobile, out var mobileUser) ? mobileUser.PlayerName : "No mobile") + "</i>";
            OnWinCountChanged(0, pcUser.WinCount.Value);
            
            pcUser.WinCount.OnValueChanged += OnWinCountChanged;
        }

        private void OnWinCountChanged(int _, int newValue)
        {
            winCountText.text = $"{newValue}";
        }
    }
}