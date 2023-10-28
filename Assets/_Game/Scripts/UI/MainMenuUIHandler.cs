using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChatGPT_Detective
{
    public class MainMenuUIHandler : MonoBehaviour
    {
        [SerializeField] private GameObject m_continueButton;

        [SerializeField] private int m_GameSceneIndex = 1;

        private void Start()
        {
            m_continueButton.SetActive(SaveSystem.DoesSaveGameExist());
        }

        public void StartNewGame()
        {
            SaveSystem.ContinueGame(false);
            SceneManager.LoadScene(m_GameSceneIndex);
        }
        
        public void ContinueGame()
        {
            SaveSystem.ContinueGame(true);
            SceneManager.LoadScene(m_GameSceneIndex);
        }
        
        public void Exit()
        {
            Application.Quit();
        }
    }
}