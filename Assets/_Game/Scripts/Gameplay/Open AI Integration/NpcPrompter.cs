using OpenAI;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
    public class NpcPrompter : MonoBehaviour
    {
        [SerializeField] private int _contextWindowSize = 5;
        [SerializeField] private int _resultSampleCount = 3;
        
        private List<DialogueChunk> _npcPromptHistory = new List<DialogueChunk>();
        private VectorCollection<DialogueChunk> _dialogueVectors = new VectorCollection<DialogueChunk>(1536);

        private OpenAIClient _embeddingsClient;

        private string _lastPrompt;
        
        private void Awake()
        {
            _embeddingsClient = new OpenAIClient();
        }
        
        private void OnEnable()
        {
            GPTPromptIntegrator.Instance.RegisterOnResponseReceived(UpdateHistory);
        }

        private void OnDisable()
        {
            GPTPromptIntegrator.Instance.DeregisterOnResponseReceived(UpdateHistory);
        }

        public async Task<List<Message>> FormatPromptRequest(string charInstructions, string newPrompt)
        {
            List<Message> validHistory = new List<Message> { new Message(Role.User, "temp") };

            EmbeddingsResponse embeddings =
                await _embeddingsClient.EmbeddingsEndpoint.CreateEmbeddingAsync(newPrompt, Model.Embedding_Ada_002);
            
            if (embeddings.Data?.Count > 0)
            {
                IReadOnlyList<double> data = embeddings.Data[0].Embedding;
                
                if (_dialogueVectors.Count > 0) 
                {
                    List<DialogueChunk> nearestChunks = _dialogueVectors.FindNearest(data.ToArray(), _resultSampleCount);

                    validHistory.AddRange(GetValidHistory(nearestChunks));
                }

                validHistory.Add(new Message(Role.User, $"{charInstructions}\n\n###\n\n{newPrompt}"));
            }
            else
            {
                Debug.LogWarning("No embeddings were generated from this prompt.");
            }

            _lastPrompt = newPrompt;
            return validHistory;
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
                await _embeddingsClient.EmbeddingsEndpoint.CreateEmbeddingAsync($"{_lastPrompt}\n{response.Content}",
                    Model.Embedding_Ada_002);
            
            if (embeddings.Data?.Count > 0)
            {
                Message lastMessage = new Message(Role.User, _lastPrompt);

                DialogueChunk newChunk = new DialogueChunk(_npcPromptHistory.Count,
                    lastMessage, response,
                    embeddings.Data[0].Embedding.ToArray());
                
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
