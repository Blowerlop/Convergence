using Sirenix.OdinInspector;
using UnityEngine;

namespace Project
{
    public class FU_Client : MonoBehaviour
    {
        [ShowInInspector] private FU_NetworkVariableReadOnly _networkVariableReadOnly = new FU_NetworkVariableReadOnly();
    }
}
