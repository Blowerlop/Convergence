using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class Test : MonoBehaviour
    {
        public NetworkRTT networkRTT;
        public TMP_Text _textPing;
        
        private void FixedUpdate()
        {
            if (networkRTT == null)
            {
                networkRTT = FindObjectOfType<NetworkRTT>();
                return;
            }
            
            _textPing.text = $"Ping: {networkRTT.networkCurrentRTT}ms" ;
        }
    }
}
