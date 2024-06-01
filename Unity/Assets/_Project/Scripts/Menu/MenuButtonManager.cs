using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project
{
    public class MenuButtonManager : MonoBehaviour
    {
        public Button GameButton, TutorialButton, QuitButton, SettingsButton;
        private void Start()
        {
            GameButton.onClick.AddListener(GoToLobby);
            TutorialButton.onClick.AddListener(PlayTutorial);
            QuitButton.onClick.AddListener(QuitGame);
            SettingsButton.onClick.AddListener(OpenSettings);
        }

        void GoToLobby()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
        }
        void PlayTutorial()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Tutorial", LoadSceneMode.Single);
        }

        void OpenSettings()
        {
            MenuManager.instance.Toggle();
        }
        void QuitGame()
        {
            Application.Quit();
        }
    }
}
