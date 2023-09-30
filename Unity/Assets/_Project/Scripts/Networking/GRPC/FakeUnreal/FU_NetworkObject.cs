using System.Collections.Generic;
using UnityEngine;

namespace Project
{
    public class FU_NetworkObject : MonoBehaviour
    {
        public ulong NetID = 0;

        public void Init(ulong netId)
        {
            NetID = netId;
        }
        
        //Set/Get NetVars and everything we need
    }
}