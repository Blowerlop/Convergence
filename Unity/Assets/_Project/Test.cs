using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class Test : MonoBehaviour
    {
        public Netcode_NetworkRTT _netcodeNetworkRTT;
        public TMP_Text _textPing;
        
        private void FixedUpdate()
        {
            if (_netcodeNetworkRTT == null)
            {
                _netcodeNetworkRTT = FindObjectOfType<Netcode_NetworkRTT>();
                return;
            }
            
            _textPing.text = $"Ping: {_netcodeNetworkRTT.networkCurrentRTT}ms" ;
        }
    }
}
