using OpenAI;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

namespace ChatGPT_Detective
{
    public class NPCPromptHandler : MonoBehaviour
    {
        [SerializeField] private CharacterInfo _characterInfo;

        [SerializeField] private int _contextWindowSize = 5;
        [SerializeField] private int _resultSampleCount = 3;
        
        private List<DialogueChunk> _npcPromptHistory = new List<DialogueChunk>();
        private VectorCollection<DialogueChunk> _dialogueVectors = new VectorCollection<DialogueChunk>(1536);

        private OpenAIApi _openAi = new OpenAIApi();

        private string _lastPrompt;

        [SerializeField] private InputField inputField;
        [SerializeField] private Button button;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private RectTransform sent;
        [SerializeField] private RectTransform received;
        private float height;
        private void Start()
        {
            button.onClick.AddListener(SendReply);
        }
        private void SendReply()
        {
            ProcessPromptRequest(inputField.text);
        }
        private void AppendMessage(ChatMessage message)
        {
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

            var item = Instantiate(message.Role == "user" ? sent : received, scroll.content);
            item.GetChild(0).GetChild(0).GetComponent<Text>().text = message.Content;
            item.anchoredPosition = new Vector2(0, -height);
            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            height += item.sizeDelta.y;
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scroll.verticalNormalizedPosition = 0;
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
            CreateEmbeddingsResponse embeddingsResponse = await _openAi.CreateEmbeddings(new CreateEmbeddingsRequest()
            {
                Model = "text-embedding-ada-002",
                Input = newPrompt
            });

            if (embeddingsResponse.Data?.Count > 0)
            {
                List<float> data = embeddingsResponse.Data[0].Embedding;
                
                List<ChatMessage> validHistory;

                if (_dialogueVectors.Count > 0) 
                {
                    List<DialogueChunk> nearestChunks =
                        _dialogueVectors.FindNearest(data.ToArray(), _resultSampleCount);

                    validHistory = GetValidHistory(nearestChunks);
                }
                else
                {
                    validHistory = new List<ChatMessage> { new ChatMessage() };
                }

                AppendMessage(new ChatMessage()
                {
                    Role = "user",
                    Content = newPrompt
                });

                validHistory.Add(new ChatMessage()
                {
                    Role = "user",
                    Content = $"{_characterInfo.GetCharacterInstructions()}\n\n###\n\n{newPrompt}"
                });

                GPTIntegrationHandler.Instance.SendPromptMessage(_characterInfo.GetCharacterInfo(), validHistory);
            }
            else
            {
                Debug.LogWarning("No embeddings were generated from this prompt.");
            }

            _lastPrompt = newPrompt;
        }

        private List<ChatMessage> GetValidHistory(List<DialogueChunk> nearestChunks)
        {
            List<ChatMessage> validHistory = new List<ChatMessage>();
            validHistory.Add(new ChatMessage());

            AssignValidDialogues(nearestChunks, validHistory);
            return validHistory;
        }

        private void AssignValidDialogues(List<DialogueChunk> nearestChunks, List<ChatMessage> validHistory)
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

        private void UpdateHistory(ChatMessage response)
        {
            UpdateHistoryAsync(response);
        }

        private async void UpdateHistoryAsync(ChatMessage response)
        {
            CreateEmbeddingsResponse embeddingsResponse = await _openAi.CreateEmbeddings(new CreateEmbeddingsRequest()
            {
                Model = "text-embedding-ada-002",
                Input = $"{_lastPrompt}\n{response.Content}"
            });

            if (embeddingsResponse.Data?.Count > 0)
            {
                ChatMessage lastMessage = new ChatMessage()
                {
                    Role = "user",
                    Content = _lastPrompt
                };

                DialogueChunk newChunk = new DialogueChunk(_npcPromptHistory.Count,
                    lastMessage, response,
                    embeddingsResponse.Data[0].Embedding.ToArray());
                AppendMessage(response);
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
