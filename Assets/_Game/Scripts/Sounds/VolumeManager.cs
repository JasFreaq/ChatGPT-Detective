using System.Collections;
using System.Collections.Generic;
using ChatGPT_Detective;
using UnityEngine;
using UnityEngine.UI;

namespace ChatGPT_Detective
{
    public class VolumeManager : MonoBehaviour
    {
        [SerializeField] private Slider m_volumeSlider;

        private SoundHandler[] m_soundHandlers;

        private void Start()
        {
            m_soundHandlers = FindObjectsByType<SoundHandler>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        public void OnVolumeChanged()
        {
            foreach (SoundHandler soundHandler in m_soundHandlers)
            {
                soundHandler.SetVolume(m_volumeSlider.value);
            }
        }
    }
}