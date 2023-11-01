using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project
{
    public class FU_NetworkObject : MonoBehaviour, IDisposable
    {
        public ulong NetID = 0;

        public void Init(ulong netId)
        {
            NetID = netId;
        }

        public void Dispose()
        {
            Destroy(gameObject);
        }
        
        //Set/Get NetVars and everything we need
    }
}