using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class TargetObject : NetworkBehaviour, ITargetable
    {
        public void OnTargeted()
        {
            Debug.Log("Targeted");
        }

        public void OnUntargeted()
        {
            Debug.Log("Untargeted");
        }

        public ulong GetID() => NetworkObjectId;
    }
}