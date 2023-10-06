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

        private ChatMessage _currentProcessedMessage;

        private void OnEnabled()
        {
            GPTIntegrationHandler.Instance.RegisterOnResponseReceived(UpdateHistory);
        }

        private void OnDisabled()
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
                List<DialogueChunk> nearestChunks = _dialogueVectors.FindNearest(data.ToArray(), _resultSampleCount);

                List<ChatMessage> validHistory = GetValidHistory(nearestChunks);

                _currentProcessedMessage = new ChatMessage()
                {
                    Role = "user",
                    Content = newPrompt
                };
                
                validHistory.Add(_currentProcessedMessage);

                GPTIntegrationHandler.Instance.SendPromptMessage(_characterInfo.GetCharacterInfo(), validHistory);
            }
            else
            {
                Debug.LogWarning("No embeddings were generated from this prompt.");
            }
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
                Input = $"{_currentProcessedMessage.Content}\n{response.Content}"
            });

            if (embeddingsResponse.Data?.Count > 0)
            {
                DialogueChunk newChunk = new DialogueChunk(_npcPromptHistory.Count,
                    _currentProcessedMessage, response,
                    embeddingsResponse.Data[0].Embedding.ToArray());

                _npcPromptHistory.Add(newChunk);
                _dialogueVectors.Add(newChunk);
            }
            else
            {
                Debug.LogWarning("No embeddings were generated from this prompt.");
            }
        }
    }
}
