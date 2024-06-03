using System;
using TMPro;
using UnityEngine;

namespace Project
{
    public class PlayerNameSetterUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _inputField;


        private void Start()
        {
            _inputField.text = PlayerData.playerName;
        }

        private void OnEnable()
        {
            _inputField.onEndEdit.AddListener(SetPlayerName);
        }
        
        private void OnDisable()
        {
            _inputField.onEndEdit.RemoveListener(SetPlayerName);
        }

        private void SetPlayerName(string name)
        {
            Debug.Log("Setting player name to: " + name);
            PlayerData.playerName = name;
        }
    }
}
