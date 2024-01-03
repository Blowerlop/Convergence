using Unity.Netcode;
using UnityEngine;

namespace Project.Spells
{
    public abstract class Spell : NetworkBehaviour
    {
        public abstract void Init(IChannelingResult channelingResult);

        public abstract (Vector3, Quaternion) GetDefaultTransform(IChannelingResult channelingResult, PlayerRefs player);
    }
}