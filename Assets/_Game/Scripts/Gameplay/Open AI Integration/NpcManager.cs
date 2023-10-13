using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI.Chat;
using OpenAI.Embeddings;
using OpenAI.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChatGPT_Detective
{
    public class NpcManager : MonoBehaviour
    {
        [SerializeField] private CharacterInfo _charInfo;

        private NpcPrompter _prompter;
        private NpcGoalsHandler _goalsHandler;
        
        #region Temp

        [SerializeField] private Button submitButton;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private RectTransform contentArea;
        [SerializeField] private ScrollRect scrollView;
        private float height;
        private void SendReply()
        {
            ProcessPromptRequest(inputField.text);
        }
        private TextMeshProUGUI AddNewTextMessageContent()
        {
            var textObject = new GameObject($"Message_{contentArea.childCount + 1}");
            textObject.transform.SetParent(contentArea, false);
            var textMesh = textObject.AddComponent<TextMeshProUGUI>();
            textMesh.fontSize = 24;
            textMesh.enableWordWrapping = true;
            return textMesh;
        }
        private void UpdateHistory(Message response)
        {
            var assistantMessageContent = AddNewTextMessageContent();
            assistantMessageContent.text = $"Assistant: {response.Content}";
        }
        private void OnEnable()
        {
            GPTPromptIntegrator.Instance.RegisterOnResponseReceived(UpdateHistory);
        }
        private void OnDisable()
        {
            GPTPromptIntegrator.Instance.DeregisterOnResponseReceived(UpdateHistory);
        }
        #endregion
        
        private void Awake()
        {
            _prompter = GetComponent<NpcPrompter>();
            _goalsHandler = GetComponent<NpcGoalsHandler>();
        }

        private void Start()
        {
            submitButton.onClick.AddListener(SendReply);

            _goalsHandler.SetupGoalHandling(_charInfo.CharGoals);
        }

        public async void ProcessPromptRequest(string newPrompt)
        {
            List<Message> validHistory = await _prompter.FormatPromptRequest(_charInfo.CharInstructions, newPrompt);

            GPTPromptIntegrator.Instance.SendPromptMessage(_charInfo.CharGoals[0].Id,
                _charInfo.CharInfo, _goalsHandler.CurrentGoal.Goal, validHistory);
        }
    }
}