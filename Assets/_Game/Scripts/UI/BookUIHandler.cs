using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ChatGPT_Detective
{
    public class BookUIHandler : MonoBehaviour
    {
        [SerializeField] private float m_bookOpenTime = 1.2f;

        [SerializeField] private GameObject m_bookUI;

        [Header("Interpolation")] 
        [SerializeField] private GameObject m_bookButton;

        [SerializeField] private RectTransform m_bookInterpIcon;

        [SerializeField] private RectTransform m_sourceTransform;

        [SerializeField] private RectTransform m_targetTransform;

        [Header("Animation")] 
        [SerializeField] private Animator m_bookAnimator;

        [SerializeField] private string m_openState = "Open";

        [SerializeField] private string m_closeState = "Close";

        [SerializeField] private string m_forwardFlipState = "Flip Forward";

        [SerializeField] private string m_reverseFlipState = "Flip Reverse";

        [SerializeField] private float m_openAnimationTime = 0.5f;

        [SerializeField] private float m_flipAnimationTime = 0.5f;

        [Header("UI")] 
        [SerializeField] private NpcHistoryButton m_npcHistoryButtonPrefab;

        [SerializeField] private GameObject m_npcButtonsPanel;

        [SerializeField] private TextMeshProUGUI m_firstPageText;

        [SerializeField] private TextMeshProUGUI m_secondPageText;

        [SerializeField] private GameObject m_npcHistoryPanel;

        [SerializeField] private GameObject[] m_flipLeftButton;

        [SerializeField] private GameObject[] m_flipRightButton;

        private NpcPrompter[] m_npcs;

        private NpcHistoryButton[] m_npcButtons;

        private Interpolator<float> m_bookAnimationInterpolator;

        private bool m_isBookOpen;

        private int m_animHashOpen;

        private int m_animHashClose;

        private int m_animHashForwardFlip;

        private int m_animHashReverseFlip;

        public bool IsBookOpen => m_isBookOpen;

        private void Awake()
        {
            m_npcs = FindObjectsByType<NpcPrompter>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            Array.Sort(m_npcs, new NpcPrompter.NpcPrompterComparer());
        }

        private void Start()
        {
            m_bookAnimationInterpolator = new Interpolator<float>(0f, 1f, m_bookOpenTime, Mathf.Lerp,
                onReachedTarget: OpenBook, onReachedDefault: DisableBook);

            AssignAnimationHashes();

            SetupNpcHistoryUI();
        }

        private void AssignAnimationHashes()
        {
            m_animHashOpen = Animator.StringToHash(m_openState);
            m_animHashClose = Animator.StringToHash(m_closeState);
            m_animHashForwardFlip = Animator.StringToHash(m_forwardFlipState);
            m_animHashReverseFlip = Animator.StringToHash(m_reverseFlipState);
        }

        private void SetupNpcHistoryUI()
        {
            m_npcButtons = new NpcHistoryButton[m_npcs.Length];

            for (int i = 0; i < m_npcs.Length; i++)
            {
                NpcHistoryButton npcHistoryButton = Instantiate(m_npcHistoryButtonPrefab, m_npcButtonsPanel.transform);

                int index = i;
                npcHistoryButton.SetupButton(() => { EnableNpcHistoryLayout(index); },
                    m_npcs[i].CharInfo.CharacterName);

                m_npcButtons[i] = npcHistoryButton;
            }
        }

        private void Update()
        {
            if (m_bookAnimationInterpolator.Interpolating)
            {
                float t = m_bookAnimationInterpolator.Update();

                m_bookInterpIcon.position = Vector3.Lerp(m_sourceTransform.position, m_targetTransform.position, t);
                m_bookInterpIcon.sizeDelta = Vector2.Lerp(m_sourceTransform.sizeDelta, m_targetTransform.sizeDelta, t);
            }
        }

        public void EnableBook()
        {
            m_bookInterpIcon.gameObject.SetActive(true);
            m_bookButton.SetActive(false);

            UICoordinator.Instance.DisablePopup();

            m_isBookOpen = true;

            m_bookAnimationInterpolator.Toggle(true);
        }

        private void DisableBook()
        {
            m_bookInterpIcon.gameObject.SetActive(false);
            m_bookButton.SetActive(true);

            m_isBookOpen = false;
        }

        private void OpenBook()
        {
            m_bookInterpIcon.gameObject.SetActive(false);

            StartCoroutine(OpenBookRoutine());
        }

        public void CloseBook()
        {
            m_bookUI.SetActive(false);

            m_npcHistoryPanel.SetActive(false);
            foreach (GameObject obj in m_flipLeftButton)
                obj.SetActive(false);
            foreach (GameObject obj in m_flipRightButton)
                obj.SetActive(false);

            StartCoroutine(CloseBookRoutine());
        }

        private IEnumerator OpenBookRoutine()
        {
            m_bookAnimator.gameObject.SetActive(true);

            m_bookAnimator.Play(m_animHashOpen);

            yield return new WaitForSeconds(m_openAnimationTime);

            yield return FlipBookAnimationRoutine(1);

            m_bookAnimator.gameObject.SetActive(false);

            EnableNpcButtonsLayout();

            m_bookUI.SetActive(true);
        }

        private IEnumerator CloseBookRoutine()
        {
            m_bookAnimator.gameObject.SetActive(true);

            yield return FlipBookAnimationRoutine(1, true);

            m_bookAnimator.Play(m_animHashClose);

            yield return new WaitForSeconds(m_openAnimationTime);

            m_bookAnimator.gameObject.SetActive(false);

            m_bookInterpIcon.gameObject.SetActive(true);

            m_bookAnimationInterpolator.Toggle(false);
        }

        private IEnumerator FlipBookAnimationRoutine(int repeat = 0, bool close = false)
        {
            m_bookAnimator.Play(close ? m_animHashReverseFlip : m_animHashForwardFlip);

            yield return new WaitForSeconds(m_flipAnimationTime * (repeat + 1));
        }

        private void EnableNpcButtonsLayout()
        {
            for (int i = 0; i < m_npcs.Length; i++)
            {
                bool interacted = m_npcs[i].History.Count > 0;
                m_npcButtons[i].gameObject.SetActive(interacted);
            }

            m_npcButtonsPanel.SetActive(true);
        }

        private void EnableNpcHistoryLayout(int index)
        {
            m_npcButtonsPanel.SetActive(false);

            IReadOnlyList<DialogueChunk> history = m_npcs[index].History;

            string name = m_npcs[index].CharInfo.CharacterName;
            string historyText = "";

            foreach (DialogueChunk chunk in history)
            {
                historyText += $"Detective: {chunk.Prompt.Content}\n";
                historyText += $"{name}: {chunk.Response.Content}\n";
            }

            m_firstPageText.text = historyText;

            StartCoroutine(EnableNpcHistoryLayoutRoutine(index));
        }

        private IEnumerator EnableNpcHistoryLayoutRoutine(int index)
        {
            m_bookAnimator.gameObject.SetActive(true);

            yield return FlipBookAnimationRoutine();

            m_bookAnimator.gameObject.SetActive(false);

            string historyText = m_firstPageText.text;

            m_npcHistoryPanel.SetActive(true);

            foreach (GameObject obj in m_flipLeftButton)
                obj.SetActive(true);

            yield return new WaitForEndOfFrame();

            if (m_firstPageText.textInfo.pageCount > 1)
            {
                int firstPageLastIndex = m_firstPageText.textInfo.pageInfo[0].lastCharacterIndex;
                m_secondPageText.text =
                    historyText.Substring(firstPageLastIndex + 1, historyText.Length - firstPageLastIndex - 1);

                m_secondPageText.gameObject.SetActive(true);

                if (m_firstPageText.textInfo.pageCount > 2)
                {
                    foreach (GameObject obj in m_flipRightButton)
                        obj.SetActive(true);
                }
            }
            else
            {
                m_secondPageText.gameObject.SetActive(false);
            }
        }

        public void FlipPageLeft()
        {
            m_npcHistoryPanel.SetActive(false);

            StartCoroutine(FlipPageLeftRoutine());
        }

        public void FlipPageRight()
        {
            m_npcHistoryPanel.SetActive(false);

            StartCoroutine(FlipPageRightRoutine());
        }

        private IEnumerator FlipPageLeftRoutine()
        {
            m_bookAnimator.gameObject.SetActive(true);

            yield return FlipBookAnimationRoutine(close: true);

            m_bookAnimator.gameObject.SetActive(false);

            if (m_firstPageText.pageToDisplay == 1)
            {
                foreach (GameObject obj in m_flipLeftButton)
                    obj.SetActive(false);

                EnableNpcButtonsLayout();
            }
            else
            {
                string historyText = m_firstPageText.text;

                m_npcHistoryPanel.SetActive(true);

                int nextPage = m_firstPageText.pageToDisplay - 2;
                m_firstPageText.pageToDisplay = nextPage;

                int firstPageLastIndex = m_firstPageText.textInfo.pageInfo[nextPage - 1].lastCharacterIndex;
                m_secondPageText.text =
                    historyText.Substring(firstPageLastIndex + 1, historyText.Length - firstPageLastIndex - 1);

                m_secondPageText.gameObject.SetActive(true);

                foreach (GameObject obj in m_flipRightButton)
                    obj.SetActive(true);
            }
        }

        private IEnumerator FlipPageRightRoutine()
        {
            m_bookAnimator.gameObject.SetActive(true);

            yield return FlipBookAnimationRoutine();

            m_bookAnimator.gameObject.SetActive(false);

            string historyText = m_firstPageText.text;

            m_npcHistoryPanel.SetActive(true);

            int nextPage = m_firstPageText.pageToDisplay + 2;
            m_firstPageText.pageToDisplay = nextPage;

            if (m_firstPageText.textInfo.pageCount > nextPage)
            {
                int firstPageLastIndex = m_firstPageText.textInfo.pageInfo[nextPage - 1].lastCharacterIndex;
                m_secondPageText.text =
                    historyText.Substring(firstPageLastIndex + 1, historyText.Length - firstPageLastIndex - 1);

                m_secondPageText.gameObject.SetActive(true);

                foreach (GameObject obj in m_flipRightButton)
                    obj.SetActive(m_firstPageText.textInfo.pageCount > nextPage + 1);
            }
            else
            {
                m_secondPageText.gameObject.SetActive(false);

                foreach (GameObject obj in m_flipRightButton)
                    obj.SetActive(false);
            }
        }
    }
}