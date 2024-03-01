using Project._Project.Scripts.Player.States;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project
{
    public class EmoteController : MonoBehaviour
    {
        [SerializeField] private PCPlayerRefs refs;

        [RequiredListLength(4)]
        [SerializeField] private EmoteData[] emotes;
        
        public void TryPlayEmote(int emoteIndex)
        {
            var state = new EmoteState(emoteIndex);
            if (refs.StateMachine.CanChangeStateTo(state))
            {
                refs.StateMachine.ChangeState(state);
            }
        }

        public EmoteData GetEmote(int index) => emotes[index];
    }
}