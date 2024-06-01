using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project
{
    public class TeamHeaderItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI teamIndexText; 
        [SerializeField] private TextMeshProUGUI teamNameText;
        [SerializeField] private TextMeshProUGUI winCountText;
        [SerializeField] private Image delimiterBar1, delimiterBar2;
        private int _teamIndex;
        
        public void Init(TeamData teamData, int teamIndex)
        {
            if (!teamData.TryGetUserInstance(PlayerPlatform.Pc, out var pcUser))
            {
                Debug.LogError($"Team {teamIndex} has no PC player!");
                return;
            }
            teamIndexText.text = $"Team {teamIndex + 1} ";
            teamNameText.text = pcUser.PlayerName + (pcUser.IsOwner ? " (You)" : "") +
                "\n<i>" + (teamData.TryGetUserInstance(PlayerPlatform.Mobile, out var mobileUser) ? mobileUser.PlayerName : "No mobile") + "</i>";
            OnWinCountChanged(0, pcUser.WinCount.Value);
            
            switch(teamIndex)
            {
                case 0:
                    SetItemColor("#00DCFF");
                    break;
                case 1:
                    SetItemColor("#C03A2C");
                    break;
                case 2:
                    SetItemColor("#D4B900");
                    break;
            }
            pcUser.WinCount.OnValueChanged += OnWinCountChanged;
        }

        private void OnWinCountChanged(int _, int newValue)
        {
            winCountText.text = $"{newValue}";
        }
        private void SetItemColor(Color color)
        {
            teamIndexText.color = color;
            teamNameText.color = color;
            winCountText.color = color;
            delimiterBar1.color = color;
            delimiterBar2.color = color;
        }

        private void SetItemColor(string htmlValue)
        {
            ColorUtility.TryParseHtmlString(htmlValue, out var color);
            teamIndexText.color = color;
            teamNameText.color = color;
            winCountText.color = color;
            delimiterBar1.color = color;
            delimiterBar2.color = color;
        }
    }
}