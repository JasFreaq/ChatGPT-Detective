using System;
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
        [SerializeField] private CharacterInfo _charInfo;

        private HistoryData _historyData = new HistoryData();

        private OpenAIClient _embeddingsClient;
        
        private NpcGoalsHandler _goalsHandler;

        private string _lastPrompt;

        public CharacterInfo CharInfo => _charInfo;

        private static int CountWords(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return 0;
            }
            
            string[] words = input.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            return words.Length;
        }

        private void Awake()
        {
            _embeddingsClient = new OpenAIClient();
            
            _goalsHandler = GetComponent<NpcGoalsHandler>();
        }

        private void Start()
        {
            _goalsHandler.SetupGoalHandling(_charInfo.CharGoals);
        }

        private void OnEnable()
        {
            GPTPromptIntegrator.Instance.RegisterOnResponseReceived(UpdateHistory);
        }

        private void OnDisable()
        {
            GPTPromptIntegrator.Instance.DeregisterOnResponseReceived(UpdateHistory);
        }

        public void ProcessPromptRequest(string newPrompt)
        {
            GPTPromptIntegrator.Instance.SendPromptMessage(_charInfo, newPrompt, _historyData, _goalsHandler.CurrentGoal);

            _lastPrompt = newPrompt;
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

                DialogueChunk newChunk = new DialogueChunk(_historyData.PromptHistory.Count,
                    lastMessage, response,
                    embeddings.Data[0].Embedding.ToArray());
                
                _historyData.Add(newChunk);

                string hist = "";
                foreach (DialogueChunk chunk in _historyData.PromptHistory)
                {
                    hist += $"User: {chunk.Prompt.Content}\nAssistant: {chunk.Response.Content}\n";
                }
                Debug.Log(hist);
            }
            else
            {
                Debug.LogWarning("No embeddings were generated from this prompt.");
            }
        }
    }
}
