using System.Collections;
using OpenAI.Chat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChatGPT_Detective
{
    public class ChatUIHandler : MonoBehaviour
    {
        [SerializeField] private GameObject m_playerInputPanel;

        [SerializeField] private GameObject m_npcResponsePanel;

        [SerializeField] private TMP_InputField m_playerInputField;

        [SerializeField] private TextMeshProUGUI m_responseText;

        [SerializeField] private TextMeshProUGUI m_npcNameText;

        [SerializeField] private Button m_npcResponseConfirmationButton;

        [SerializeField] private float m_streamInterpolationSpeed = 0.1f;

        private NpcPrompter m_npcPrompter;

        private string m_streamResponseTargetString;

        private Coroutine m_streamResponseCoroutine;

        private void Start()
        {
            m_npcResponseConfirmationButton.onClick.AddListener(ConfirmNpcResponse);
        }

        private void OnEnable()
        {
            GPTPromptIntegrator.Instance.RegisterOnResponseStreaming(StreamNpcResponse);
            GPTPromptIntegrator.Instance.RegisterOnResponseReceived(UpdateNpcResponse);
        }

        private void OnDisable()
        {
            GPTPromptIntegrator.Instance.DeregisterOnResponseStreaming(StreamNpcResponse);
            GPTPromptIntegrator.Instance.DeregisterOnResponseReceived(UpdateNpcResponse);
        }

        public void EnableChat(int id)
        {
            NpcPrompter prompter = NpcDataCache.Instance.GetPrompter(id);

            if (prompter != null)
            {
                m_npcPrompter = prompter;
                m_npcPrompter.enabled = true;

                m_npcNameText.text = m_npcPrompter.CharInfo.CharacterName;

                m_playerInputPanel.SetActive(true);
            }
        }

        public void DisableChat()
        {
            m_playerInputPanel.SetActive(false);
            m_npcResponsePanel.SetActive(false);

            m_npcPrompter.enabled = false;
            m_npcPrompter = null;
        }

        public void SendPlayerInput()
        {
            if (m_playerInputField.text != string.Empty)
            {
                m_npcPrompter.ProcessPromptRequest(m_playerInputField.text);

                m_streamResponseTargetString = string.Empty;
                m_responseText.text = "...";

                m_playerInputPanel.SetActive(false);
                m_npcResponsePanel.SetActive(true);

                m_npcResponseConfirmationButton.gameObject.SetActive(false);
            }
        }

        public void ClearPlayerInput()
        {
            m_playerInputField.text = "";
        }

        private void StreamNpcResponse(string response)
        {
            if (string.IsNullOrEmpty(m_streamResponseTargetString))
            {
                m_responseText.text = string.Empty;

                m_streamResponseTargetString = response;
                m_streamResponseCoroutine = StartCoroutine(StringInterpolationRoutine());
            }
            else
            {
                m_streamResponseTargetString += response;

                m_streamResponseCoroutine ??= StartCoroutine(StringInterpolationRoutine());
            }
        }

        private void UpdateNpcResponse(Message _)
        {
            m_npcResponseConfirmationButton.gameObject.SetActive(true);
        }

        private void ConfirmNpcResponse()
        {
            if (m_responseText.text.Length < m_streamResponseTargetString.Length)
            {
                if (m_streamResponseCoroutine != null)
                {
                    StopCoroutine(m_streamResponseCoroutine);
                    m_streamResponseCoroutine = null;
                }

                m_responseText.text = m_streamResponseTargetString;
            }
            else
            {
                m_playerInputField.text = string.Empty;

                m_playerInputPanel.SetActive(true);
                m_npcResponsePanel.SetActive(false);
            }
        }

        private IEnumerator StringInterpolationRoutine()
        {
            int maxLength = m_streamResponseTargetString.Length;
            int i = m_responseText.text.Length;

            while (i < maxLength)
            {
                string interpolatedString = m_responseText.text;

                interpolatedString += m_streamResponseTargetString[i];

                m_responseText.text = interpolatedString;

                yield return new WaitForSeconds(m_streamInterpolationSpeed);

                i++;
                maxLength = m_streamResponseTargetString.Length;
            }

            m_streamResponseCoroutine = null;
        }
    }
}
