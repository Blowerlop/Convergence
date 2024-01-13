using Unity.Netcode;
using UnityEngine;

namespace Project.Spells
{
    public abstract class Spell : NetworkBehaviour
    {
        public abstract void Init(ICastResult castResult);

        public abstract (Vector3, Quaternion) GetDefaultTransform(ICastResult castResult, PlayerRefs player);
    }
}