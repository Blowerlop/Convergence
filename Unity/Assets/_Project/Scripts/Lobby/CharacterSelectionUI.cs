using Sirenix.OdinInspector;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Project
{
    public struct PlayerLobbyData
    {
        public int _teamIndex;
        public int characterId;
        public bool isReady;
    }

    public class PlayerLobbyDataManagement
    {
        public PlayerLobbyData playerLobbyData;
    }
    
    public class CharacterSelectionUI : NetworkBehaviour
    {
        [SerializeField, Required] private SOCharacter _characterData;

        [SerializeField, Required] private Image _image;
        [SerializeField, Required] private TMP_Text _name;

        [ClearOnReload, ShowInInspector] private static int _characterSelectedId;

        [ClearOnReload(assignNewTypeInstance: true, nameof(onCharacterSelectedEvent))] public static Event<int, int> onCharacterSelectedEvent = new Event<int, int>(nameof(onCharacterSelectedEvent));

        [Button]
        private void OnValidate()
        {
            _image.sprite = _characterData.avatar;
            _name.text = _characterData.characterName;

            name = _characterData.characterName;
        }

        public void SelectCharacter() => _characterSelectedId = _characterData.id;
        
        public void SetCharacterServerRpcc()
        {
            SetCharacterServerRpc((int)NetworkManager.Singleton.LocalClientId, _characterSelectedId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetCharacterServerRpc(int clientId, int characterId)
        {
            Debug.Log("ici");
            UserInstance userInstance = UserInstanceManager.instance.GetUserInstance(clientId);
            if (userInstance.CharacterId == characterId) return;
            
            userInstance.SetCharacterServerRpc(characterId);
            BlablaClientRpc(clientId, characterId);
            
            onCharacterSelectedEvent.Invoke(this, false, clientId, characterId);
        }

        [ClientRpc]
        private void BlablaClientRpc(int clientId, int characterId)
        {
            
        }
    }
}
