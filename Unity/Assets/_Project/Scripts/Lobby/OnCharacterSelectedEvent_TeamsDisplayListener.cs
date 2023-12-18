using UnityEngine;

namespace Project
{
    public class OnCharacterSelectedEvent_TeamsDisplayListener : MonoBehaviour
    {
        private TeamsDisplay _teamsDisplay;


        private void Awake()
        {
            _teamsDisplay = FindObjectOfType<TeamsDisplay>();
        }

        private void Start()
        {
            CharacterSelectionUI.onCharacterSelectedEvent.Subscribe(this, OnCharacterSelected_SetPlayerAvatar);
        }

        private void OnDestroy()
        {
            CharacterSelectionUI.onCharacterSelectedEvent.Unsubscribe(OnCharacterSelected_SetPlayerAvatar);
        }

        private void OnCharacterSelected_SetPlayerAvatar(int playerId, int characterId)
        {
            _teamsDisplay.SetPlayerCharacterAvatarServerRpc(playerId, characterId);
        }
    }
}
