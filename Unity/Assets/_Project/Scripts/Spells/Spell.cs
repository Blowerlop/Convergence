using Unity.Netcode;
using UnityEngine;

namespace Project.Spells
{
    public abstract class Spell : NetworkBehaviour
    {
        protected int CasterTeamIndex { get; set; }

        protected SpellData Data { get; private set; }
        
        // Called by Server
        // Used to set field that are common for every spell before calling overriden Init
        [Server]
        public void Init(ICastResult castResult, SpellData data, int teamIndex)
        {
            CasterTeamIndex = teamIndex;
            Data = data;
            
            Init(castResult);
        }
        
        protected abstract void Init(ICastResult castResult);

        public abstract (Vector3, Quaternion) GetDefaultTransform(ICastResult castResult, PlayerRefs player);
        
        public abstract Vector3 GetDirection(ICastResult castResult, PlayerRefs player);
    }
}