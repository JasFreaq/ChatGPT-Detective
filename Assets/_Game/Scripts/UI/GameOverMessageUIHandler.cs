using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChatGPT_Detective
{
    public class GameOverMessageUIHandler : MonoBehaviour
    {
        [SerializeField] private GameObject m_gameOverPanel;

        public void ProcessGameOver()
        {
            Time.timeScale = 0f;

            m_gameOverPanel.SetActive(true);
        }
    }
}