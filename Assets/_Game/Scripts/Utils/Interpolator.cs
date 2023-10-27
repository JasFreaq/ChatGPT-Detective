using System;
using UnityEngine;

namespace ChatGPT_Detective
{
    public class Interpolator<T>
    {
        private Tuple<T, T> m_interpValues;

        private T m_defaultVal;

        private T m_targetVal;

        private float m_interpTime;

        private float m_interpTimer;

        private bool m_interpolating;

        private bool m_inTargetState;

        private bool m_interpolatingTowardsTargetState;

        private Func<T, T, float, T> m_interpFunc;

        private Action m_onToggledOn;

        private Action m_onToggledOff;

        private Action m_onReachedTargetState;

        private Action m_onReachedDefaultState;

        public T DefaultVal
        {
            set => m_defaultVal = value;
        }

        public T TargetVal
        {
            set => m_targetVal = value;
        }

        public bool Interpolating => m_interpolating;

        public Interpolator(T defaultVal, T targetVal, float interpTime,
            Func<T, T, float, T> interpFunc,
            Action onToggledOn = null, Action onToggledOff = null,
            Action onReachedTarget = null, Action onReachedDefault = null)
        {
            m_defaultVal = defaultVal;
            m_targetVal = targetVal;
            m_interpTime = interpTime;
            m_interpFunc = interpFunc;
            m_onToggledOn += onToggledOn;
            m_onToggledOff += onToggledOff;
            m_onReachedTargetState += onReachedTarget;
            m_onReachedDefaultState += onReachedDefault;
        }

        public T Update()
        {
            m_interpTimer += Time.deltaTime;
            float t = m_interpTimer / m_interpTime;
            T value = m_interpFunc.Invoke(m_interpValues.Item1, m_interpValues.Item2, t);

            if (t >= 1f)
            {
                value = m_interpValues.Item2;
                m_interpolating = false;

                if (m_interpolatingTowardsTargetState)
                {
                    m_inTargetState = true;
                    m_onReachedTargetState?.Invoke();
                }
                else
                {
                    m_inTargetState = false;
                    m_onReachedDefaultState?.Invoke();
                }
            }

            return value;
        }

        public void Toggle(bool enable)
        {
            bool interpToTarget = enable && !m_inTargetState && !m_interpolatingTowardsTargetState;
            bool interpToDefault = !enable && m_inTargetState && m_interpolatingTowardsTargetState;

            if (interpToTarget || interpToDefault)
            {
                m_interpTimer = m_interpolating ? m_interpTime - m_interpTimer : 0;

                if (interpToTarget)
                {
                    m_interpolatingTowardsTargetState = true;
                    m_interpValues = new Tuple<T, T>(m_defaultVal, m_targetVal);
                    m_onToggledOn?.Invoke();
                }
                else
                {
                    m_interpolatingTowardsTargetState = false;
                    m_interpValues = new Tuple<T, T>(m_targetVal, m_defaultVal);
                    m_onToggledOff?.Invoke();
                }

                m_interpolating = true;
            }
        }

        public void Reset()
        {
            m_interpolating = false;
            m_inTargetState = false;
            m_interpolatingTowardsTargetState = false;
            m_interpTimer = 0;
        }
    }
}
