using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChatGPT_Detective
{
    public class PauseUIHandler : MonoBehaviour
    {
        [SerializeField] private GameObject m_pausePanel;

        [SerializeField] private int m_mainMenuIndex = 0;

        private bool m_isPaused;

        public void TogglePause()
        {
            if (!m_isPaused)
            {
                m_pausePanel.SetActive(true);
                Time.timeScale = 0;
            }
            else
            {
                m_pausePanel.SetActive(false);
                Time.timeScale = 1;
            }

            m_isPaused = !m_isPaused;
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1;

            SceneManager.LoadScene(m_mainMenuIndex);
        }

        public void Save()
        {
            GameSaveManager.Instance.SaveGameData();
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}