using System.Collections.Generic;
using OpenAI.Chat;
using UnityEngine;

namespace ChatGPT_Detective
{
    public class NpcInteractionHandler : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private int m_randomIdleStates = 2;

        [SerializeField] private int m_reactionStates = 3;

        private Animator m_animator;
        
        private Interpolator<Quaternion> m_interactionViewInterpolator;

        private List<int> m_randomIdleHashes = new List<int>();

        private List<int> m_reactionHashes = new List<int>();

        private float m_interactionViewInterpolationTime;
        
        private int m_baseIdleHash;

        private bool m_disableRandomIdle;

        public float InteractionViewInterpolationTime
        {
            set => m_interactionViewInterpolationTime = value;
        }

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
        }

        private void Start()
        {
            AssignAnimationHashes();

            m_interactionViewInterpolator = new Interpolator<Quaternion>(transform.rotation, Quaternion.identity,
                m_interactionViewInterpolationTime, Quaternion.Slerp, InterruptRandomIdle,
                onReachedDefault: () => m_disableRandomIdle = false);
        }

        private void Update()
        {
            if (!m_disableRandomIdle)
                PlayRandomIdle();

            if (m_interactionViewInterpolator.Interpolating)
            {
                transform.rotation = m_interactionViewInterpolator.Update();
            }
        }

        private void AssignAnimationHashes()
        {
            m_baseIdleHash = Animator.StringToHash("Idle0");

            for (int i = 0; i < m_randomIdleStates; i++)
            {
                int hash = Animator.StringToHash($"Idle{i + 1}");
                m_randomIdleHashes.Add(hash);
            }

            for (int i = 0; i < m_reactionStates; i++)
            {
                int hash = Animator.StringToHash($"Reaction{i + 1}");
                m_reactionHashes.Add(hash);
            }
        }

        private void PlayRandomIdle()
        {
            if (Random.Range(0, 3600) < 1)
            {
                AnimatorStateInfo currentState = m_animator.GetCurrentAnimatorStateInfo(0);

                if (currentState.shortNameHash == m_baseIdleHash)
                {
                    int randomIndex = Random.Range(0, m_randomIdleStates);

                    m_animator.Play(m_randomIdleHashes[randomIndex]);
                }
            }
        }

        public void PlayRandomReaction(Message _)
        {
            int randomIndex = Random.Range(0, m_reactionStates);

            m_animator.Play(m_reactionHashes[randomIndex]);
        }

        private void InterruptRandomIdle()
        {
            m_disableRandomIdle = true;

            m_animator.Play(m_baseIdleHash);
        }

        public void ToggleInteractionView(bool enable, Vector3 viewPosition = new Vector3())
        {
            if (enable)
            {
                Vector3 lookDirection = viewPosition - transform.position;

                m_interactionViewInterpolator.TargetVal = Quaternion.LookRotation(lookDirection);

                m_interactionViewInterpolator.Toggle(true);

                GPTPromptIntegrator.Instance.RegisterOnResponseReceived(PlayRandomReaction);
            }
            else
            {
                m_interactionViewInterpolator.Toggle(false);

                GPTPromptIntegrator.Instance.DeregisterOnResponseReceived(PlayRandomReaction);
            }
        }
    }
}
