

using UnityEngine;

namespace Project._Project.Scripts.Spells.Results
{
    public struct DefaultSkillShotResults : ICastResult
    {
        public Vector3 Direction;
        
        public override string ToString()
        {
            return $"DefaultSkillShotResults: Direction: {Direction}";
        }
    }
}