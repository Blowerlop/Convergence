using System;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Project
{
    public class CharacterSelectionUI : NetworkBehaviour
    {
        [SerializeField, Required] private SOCharacter _characterData;

        [SerializeField, Required] private Image _image;
        [SerializeField, Required] private TMP_Text _name;

        [ClearOnReload, ShowInInspector] private static int _characterSelectedId;

        [ClearOnReload] public static Action<int, int> onCharacterSelectedEvent;

        [Button]
        private void OnValidate()
        {
            _image.sprite = _characterData.avatar;
            _name.text = _characterData.characterName;

            name = _characterData.characterName;
        }

        public void SelectCharacter()
        {
            _characterSelectedId = _characterData.id;
            SelectCharacterServerRpc((int)NetworkManager.Singleton.LocalClientId, _characterSelectedId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SelectCharacterServerRpc(int clientId, int characterId)
        {
            onCharacterSelectedEvent?.Invoke(clientId, characterId);
        }

        public void ValidateCharacterServerRpcc()
        {
            ValidateCharacterServerRpc((int)NetworkManager.Singleton.LocalClientId, _characterSelectedId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ValidateCharacterServerRpc(int clientId, int characterId)
        {
            UserInstance userInstance = UserInstanceManager.instance.GetUserInstance(clientId);
            if (userInstance.CharacterId == characterId) return;
            
            userInstance.SetCharacter(characterId);
            
            SelectCharacterServerRpc(clientId, characterId);
        }
    }
}
