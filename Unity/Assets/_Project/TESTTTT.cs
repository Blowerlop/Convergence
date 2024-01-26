using Project.Spells;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project
{
    public class TESTTTT : MonoBehaviour
    {
        [Button]
        [Server]
        private void TESTNORETURN()
        {
            return;
        }
        
        [Button]
        [Server]
        private bool TESTRETURN()
        {
            return false;
        }
        
        [Button]
        [Server]
        private SpellManager TESTRETURNREF()
        {
            return null;
        }
        
        [Button]
        [Server]
        private string TESTRETURNSTR()
        {
            return "false";
        }
    }
}
