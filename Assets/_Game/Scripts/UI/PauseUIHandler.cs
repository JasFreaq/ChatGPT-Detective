using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseUIHandler : MonoBehaviour
{
    [SerializeField] private GameObject _pausePanel;

    private bool _paused;

    public void TogglePause()
    {
        if (!_paused)
        {
            _pausePanel.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            _pausePanel.SetActive(false);
            Time.timeScale = 1;
        }

        _paused = !_paused;
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
