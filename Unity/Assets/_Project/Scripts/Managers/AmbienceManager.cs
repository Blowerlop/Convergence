using System.Collections;
using UnityEngine;

namespace Project._Project.Scripts.Managers
{
    public class AmbienceManager : MonoBehaviour
    {
        enum Mode
        {
            Calm,
            Tension,
            Epic,
            Dead,
        }
        private PlayerStats _stats;

        private Mode _actualMode;

        private int _track;
        
        void Awake()
        {
            UserInstance.Me.OnPlayerLinked += LinkPlayer;
        }

        void LinkPlayer(PlayerRefs playerRefs)
        {
            if (playerRefs is PCPlayerRefs refs)
            {
                _stats = refs.Entity.Stats;
            }
            else
            {
                Debug.LogError("Trying to link a non-PCStats to Ambiance Manager!");
                return;
            }
            
            SoundManager.instance.PlayGlobalSound("Kultiran", "music", SoundManager.EventType.Music);
            StartCoroutine(NextTrackCoroutine());
            
            _stats.health.OnValueChanged += HpChanged;
        }

        void HpChanged(int current, int max)
        {
            switch ((float)current / max)
            {
                case 0:
                {
                    if (_actualMode != Mode.Dead)
                    {

                        _actualMode = Mode.Dead;
                        SoundManager.instance.SetGlobalParameterValue("Mode", (int)_actualMode);
                    }
                    break;
                }case <= 0.2f:
                {
                    if (_actualMode != Mode.Epic)
                    {
                        _actualMode = Mode.Epic;
                        SoundManager.instance.SetGlobalParameterValue("Mode", (int)_actualMode);
                    }

                    break;
                }
                case <= 0.8f:
                {
                    if (_actualMode != Mode.Tension)
                    {
                        _actualMode = Mode.Tension;
                        SoundManager.instance.SetGlobalParameterValue("Mode", (int)_actualMode);
                    }

                    break;
                }
                default:
                {
                    if (_actualMode != Mode.Calm)
                    {
                        _actualMode = Mode.Epic;
                        SoundManager.instance.SetGlobalParameterValue("Mode", (int)_actualMode);
                    }
                    break;
                }
            }
        }

        private IEnumerator NextTrackCoroutine() //TODO DO
        {
            yield return new WaitForSeconds(200);
            
            SoundManager.instance.SetGlobalParameterValue("Track", 1);
            
            yield return new WaitForSeconds(200);
            
            SoundManager.instance.SetGlobalParameterValue("Track", 0);

            yield return null;
        }
        
        private void OnDestroy()
        {
            if(UserInstance.Me != null)
                UserInstance.Me.OnPlayerLinked -= LinkPlayer;
            if(_stats != null)
                _stats.health.OnValueChanged -= HpChanged;
        }
    }
}
