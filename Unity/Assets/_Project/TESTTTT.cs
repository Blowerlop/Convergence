using Project.Spells;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project
{
    public class TESTTTT : MonoBehaviour
    {
        [Button]
        [Server]
        private void TestNoReturn()
        {
            Debug.Log("Coucou no return");
            return;
        }
        
        [Button]
        [Server]
        private bool TestReturnBool()
        {
            Debug.Log("Coucou bool");
            return false;
        }
        
        [Button]
        [Server]
        private SpellManager TestReturnRef()
        {
            Debug.Log("Coucou null");
            return null;
        }
        
        [Button]
        [Server]
        private string TestReturnString()
        {
            Debug.Log("Coucou string");
            return "false";
        }
    }
}
