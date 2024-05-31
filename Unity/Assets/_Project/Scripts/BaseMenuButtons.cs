using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project
{
    public class BaseMenuButtons : MonoBehaviour
    {
        public Button GameButton, TutorialButton, QuitButton;
        public TextMeshProUGUI gameDesc, TutorialDesc; 
        private void Start()
        {
            GameButton.onClick.AddListener(GoToLobby);
            TutorialButton.onClick.AddListener(PlayTutorial);
            QuitButton.onClick.AddListener(QuitGame);
            gameDesc.gameObject.SetActive(false);
            TutorialDesc.gameObject.SetActive(false);

        }

        void GoToLobby()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
        }
        void PlayTutorial()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Tutorial", LoadSceneMode.Single);
        }

        void QuitGame()
        {
            Application.Quit();
        }
    }
}
