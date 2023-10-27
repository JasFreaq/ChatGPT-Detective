using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChatGPT_Detective
{
    public class PauseUIHandler : MonoBehaviour
    {
        [SerializeField] private GameObject m_pausePanel;

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

        public void GoToMainMenu()
        {
            SceneManager.LoadScene(0);
        }

        public void Save()
        {
            SaveSystem.Instance.SavePlayerData();
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}