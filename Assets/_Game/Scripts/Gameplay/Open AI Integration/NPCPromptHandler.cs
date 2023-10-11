using OpenAI;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenAI.Chat;
using OpenAI.Embeddings;
using OpenAI.Models;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChatGPT_Detective
{
    public class NPCPromptHandler : MonoBehaviour
    {
        [SerializeField] private CharacterInfo _characterInfo;

        [SerializeField] private int _contextWindowSize = 5;
        [SerializeField] private int _resultSampleCount = 3;
        
        private List<DialogueChunk> _npcPromptHistory = new List<DialogueChunk>();
        private VectorCollection<DialogueChunk> _dialogueVectors = new VectorCollection<DialogueChunk>(1536);

        private OpenAIClient _openAi;

        private string _lastPrompt;

        private void Awake()
        {
            _openAi = new OpenAIClient();
        }

        [SerializeField] private Button submitButton;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private RectTransform contentArea;
        [SerializeField] private ScrollRect scrollView;
        private float height;
        private void Start()
        {
            submitButton.onClick.AddListener(SendReply);
        }
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

        private void OnEnable()
        {
            GPTIntegrationHandler.Instance.RegisterOnResponseReceived(UpdateHistory);
        }

        private void OnDisable()
        {
            GPTIntegrationHandler.Instance.DeregisterOnResponseReceived(UpdateHistory);
        }

        public async void ProcessPromptRequest(string newPrompt)
        {
            EmbeddingsResponse embeddings =
                await _openAi.EmbeddingsEndpoint.CreateEmbeddingAsync(newPrompt, Model.Embedding_Ada_002);
            
            if (embeddings.Data?.Count > 0)
            {
                IReadOnlyList<double> data = embeddings.Data[0].Embedding;

                List<Message> validHistory = new List<Message> { new Message(Role.User, "temp") };

                if (_dialogueVectors.Count > 0) 
                {
                    List<DialogueChunk> nearestChunks = _dialogueVectors.FindNearest(data.ToArray(), _resultSampleCount);

                    validHistory.AddRange(GetValidHistory(nearestChunks));
                }

                var userMessageContent = AddNewTextMessageContent();
                userMessageContent.text = $"User: {newPrompt}";

                validHistory.Add(new Message(Role.User, $"{_characterInfo.GetCharacterInstructions()}\n\n###\n\n{newPrompt}"));
               
                GPTIntegrationHandler.Instance.SendPromptMessage(_characterInfo.GetCharacterInfo(), "", validHistory);
            }
            else
            {
                Debug.LogWarning("No embeddings were generated from this prompt.");
            }

            _lastPrompt = newPrompt;
        }

        private List<Message> GetValidHistory(List<DialogueChunk> nearestChunks)
        {
            List<Message> validHistory = new List<Message>();

            AssignValidDialogues(nearestChunks, validHistory);
            return validHistory;
        }

        private void AssignValidDialogues(List<DialogueChunk> nearestChunks, List<Message> validHistory)
        {
            bool[] addedDialogues = new bool[_npcPromptHistory.Count];

            foreach (DialogueChunk chunk in nearestChunks)
            {
                Vector2Int window = new Vector2Int(chunk.HistoryIndex - _contextWindowSize,
                    chunk.HistoryIndex + _contextWindowSize + 1);

                if (window.x < 0)
                    window.x = 0;

                if (window.y > _npcPromptHistory.Count)
                    window.y = _npcPromptHistory.Count;

                for (int i = window.x; i < window.y; i++)
                {
                    if (!addedDialogues[i])
                    {
                        validHistory.Add(_npcPromptHistory[i].Prompt);
                        validHistory.Add(_npcPromptHistory[i].Response);
                    }
                }
            }
        }

        private void UpdateHistory(Message response)
        {
            UpdateHistoryAsync(response);
        }

        private async void UpdateHistoryAsync(Message response)
        {
            EmbeddingsResponse embeddings =
                await _openAi.EmbeddingsEndpoint.CreateEmbeddingAsync($"{_lastPrompt}\n{response.Content}",
                    Model.Embedding_Ada_002);
            
            if (embeddings.Data?.Count > 0)
            {
                Message lastMessage = new Message(Role.User, _lastPrompt);

                DialogueChunk newChunk = new DialogueChunk(_npcPromptHistory.Count,
                    lastMessage, response,
                    embeddings.Data[0].Embedding.ToArray());
                var assistantMessageContent = AddNewTextMessageContent();
                assistantMessageContent.text = $"Assistant: {response.Content}";
                _npcPromptHistory.Add(newChunk);
                _dialogueVectors.Add(newChunk);

                string chu = "";
                foreach (DialogueChunk chunk in _npcPromptHistory)
                {
                    chu += $"Player: {chunk.Prompt.Content}\nNPC: {chunk.Response.Content}\n";
                }
                Debug.Log(chu);
            }
            else
            {
                Debug.LogWarning("No embeddings were generated from this prompt.");
            }
        }
    }
}
