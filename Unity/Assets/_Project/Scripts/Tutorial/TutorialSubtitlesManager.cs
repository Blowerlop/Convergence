using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;


namespace Project
{
    public class TutorialSubtitlesManager : MonoBehaviour
    {

        void Start()
        {
            AudioHelper.OnTimestampReached += ReactToMessage;
        }

        private void OnDestroy()
        {
            AudioHelper.OnTimestampReached -= ReactToMessage;
        }


        void ReactToMessage(string message)
        {
            
            switch(message)
            {
                case "Test":
                    Debug.Log("SET SUBTITILES");
                    break;
                default:
                    break;
            }
        }
    }
}
