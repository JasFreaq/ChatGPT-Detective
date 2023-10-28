using UnityEngine;

namespace ChatGPT_Detective
{
    public class SoundEffectsHandler : SoundHandler
    {
        [SerializeField] private AudioClip[] m_audioClips;

        [SerializeField] private bool m_areClipsPlayedFromRandomTime;
        
        private float m_clipRandomTimeBuffer = 1f;

        public bool AreClipsPlayedFromRandomTime => m_areClipsPlayedFromRandomTime;

        public float ClipRandomTimeBuffer
        {
            get => m_clipRandomTimeBuffer;

            set => m_clipRandomTimeBuffer = value;
        }

        public void PlaySound()
        {
            if (!m_audioSource.isPlaying)
            {
                AudioClip clip = m_audioClips[Random.Range(0, m_audioClips.Length)];

                m_audioSource.clip = clip;
                m_audioSource.Play();

                if (m_areClipsPlayedFromRandomTime && clip.length > m_clipRandomTimeBuffer)
                {
                    m_audioSource.time = Random.Range(0f, clip.length - m_clipRandomTimeBuffer);
                }
            }
        }

        public void StopSound()
        {
            if (m_audioSource.isPlaying)
            {
                m_audioSource.Stop();
            }
        }
    }
}