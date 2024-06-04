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
           Initialize(); 
        }

        void Initialize()
        {
            var users = UserInstanceManager.instance.GetUsersInstance();

            for(int i = 0; i < users.Length; i++)
            {
                if (users[i].Team == playerRefs.TeamIndex && !users[i].IsMobile)
                {
                    playerPseudoText.text = users[i].PlayerName;
                    return;
                }
            }

                
            playerPseudoText.text = string.Empty;
        }

    }
}   