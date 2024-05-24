using Project._Project.Scripts.Managers;
using UnityEngine;

namespace Project
{
    public class SoundAnimEventHandler : MonoBehaviour
    {
        public void PlayStaticSound(string eventId)
        {
            SoundManager.instance.PlaySingleSound(eventId, gameObject, SoundManager.EventType.Spell);
        }
    }
}
