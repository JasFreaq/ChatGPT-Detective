using UnityEngine;

namespace ChatGPT_Detective
{
    [RequireComponent(typeof(AudioSource))]
    public abstract class SoundHandler : MonoBehaviour
    {
        protected AudioSource m_audioSource;

        private float m_maxVolume;

        protected void Awake()
        {
            m_audioSource = GetComponent<AudioSource>();
        }

        protected void Start()
        {
            m_maxVolume = m_audioSource.volume;
        }

        public void SetVolume(float ratio)
        {
            m_audioSource.volume = Mathf.Lerp(0f, m_maxVolume, ratio);
        }
    }
}