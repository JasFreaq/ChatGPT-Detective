using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChatGPT_Detective
{
    public class BackgroundMusicHandler : SoundHandler
    {
        [SerializeField] private AudioClip[] m_bgmAudioClips;

        private int _lastClipIndex = -1;

        private void Awake()
        {
            BackgroundMusicHandler[] bgmHandlers =
                FindObjectsByType<BackgroundMusicHandler>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (bgmHandlers.Length > 1)
            {
                Destroy(gameObject);
            }

            base.Awake();

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            base.Start();

            PlayMusic();
        }

        private void Update()
        {
            if (!m_audioSource.isPlaying)
            {
                PlayMusic();
            }
        }

        private void PlayMusic()
        {
            int clipIndex = Random.Range(0, m_bgmAudioClips.Length);
            while (clipIndex == _lastClipIndex)
            {
                clipIndex = Random.Range(0, m_bgmAudioClips.Length);
            }

            AudioClip clip = m_bgmAudioClips[clipIndex];

            m_audioSource.clip = clip;
            m_audioSource.Play();
        

            _lastClipIndex = clipIndex;
        }
    }
}