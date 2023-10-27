using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChatGPT_Detective
{
    public class FaderUIHandler : MonoBehaviour
    {
        [SerializeField] private Image m_fadeImage;
        [SerializeField] private float m_fadeTime = 1f;

        Interpolator<float> m_fadeInterpolator;

        private Action m_onFullyFaded;

        private void Start()
        {
            m_fadeInterpolator =
                new Interpolator<float>(0f, 1f, m_fadeTime, Mathf.Lerp,
                    () => m_fadeImage.gameObject.SetActive(true), onReachedTarget: OnFullyFaded,
                    onReachedDefault: () => m_fadeImage.gameObject.SetActive(false));
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

        public void ToggleFader(bool enable, Action onFullyFaded = null)
        {
            m_fadeInterpolator.Toggle(enable);

            m_onFullyFaded = onFullyFaded;
        }

        private void OnFullyFaded()
        {
            m_onFullyFaded?.Invoke();
        }
    }
}