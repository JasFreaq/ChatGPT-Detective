using UnityEngine;
using UnityEngine.UI;

namespace ChatGPT_Detective
{
    public class TempFaderUIHandler : MonoBehaviour
    {
        [SerializeField] private float m_fadeTime = 1.5f;

        private Image m_fadeImage;

        private Interpolator<float> m_fadeInterpolator;

        private void Awake()
        {
            m_fadeImage = GetComponent<Image>();
        }
        
        void Start()
        {
            m_fadeInterpolator =
                new Interpolator<float>(1f, 0f, m_fadeTime, Mathf.Lerp,
                    () => m_fadeImage.gameObject.SetActive(true), onReachedTarget: () => Destroy(gameObject));

            m_fadeInterpolator.Toggle(true);
        }
        
        private void Update()
        {
            if (m_fadeInterpolator.Interpolating)
            {
                Color fadeColor = m_fadeImage.color;

                fadeColor.a = m_fadeInterpolator.Update();

                m_fadeImage.color = fadeColor;
            }
        }
    }
}