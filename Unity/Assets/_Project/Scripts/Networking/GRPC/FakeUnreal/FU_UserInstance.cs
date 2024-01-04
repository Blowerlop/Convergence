using Sirenix.OdinInspector;
using UnityEngine;

namespace Project
{
    public class FU_UserInstance : FU_NetworkObject
    {
        [ShowInInspector] private readonly FU_NetworkVariableReadOnly<string> _name = new("Name");
        [ShowInInspector] private readonly FU_NetworkVariableReadOnly<int> _team = new("Team");

        private void Start()
        {
            _name.Initialize(this);
            _team.Initialize(this);
            
            _name.OnValueChanged += OnNameChanged;
            _team.OnValueChanged += OnTeamChanged;
        }
        
        private void OnNameChanged(string n)
        {
            Debug.Log($"FU_UserInstance > Name changed to {n}");
        }
        
        private void OnTeamChanged(int t)
        {
            Debug.Log($"FU_UserInstance > Team changed to {t}");
        }
    }
}