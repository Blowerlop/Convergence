using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

namespace Project
{
    public class PCPlayerPseudo : MonoBehaviour
    {
        [SerializeField] PCPlayerRefs playerRefs;
        public TextMeshProUGUI playerPseudoText;

        CancellationTokenSource cts; 
        void Start()
        {
            _ = Initialize(); 
        }

        async UniTask Initialize()
        {
            TeamData team = new TeamData();
            cts = new CancellationTokenSource(5000);
            await UniTask.WaitUntil(() => TeamManager.instance.TryGetTeam(playerRefs.TeamIndex, out team), PlayerLoopTiming.FixedUpdate, cts.Token);

            if (team.TryGetUserInstance(PlayerPlatform.Pc, out var pcUser))
                playerPseudoText.text = pcUser.PlayerName;
            else
                playerPseudoText.text = string.Empty;
        }

    }
}   