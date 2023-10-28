using System.Collections;
using System.Collections.Generic;
using ChatGPT_Detective;
using UnityEngine;
using UnityEngine.Events;

public class WelcomeMessageUIHandler : MonoBehaviour
{
    private void Start()
    {
        if (SaveSystem.IsGameContinuing())
        {
            gameObject.SetActive(false);
        }
    }
}
