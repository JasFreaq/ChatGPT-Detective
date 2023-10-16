using System;
using OpenAI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using System.Data;
using Newtonsoft.Json.Linq;
using OpenAI.Chat;
using OpenAI.Models;
using Newtonsoft.Json;
using OpenAI.Embeddings;
using System.Linq;
using UnityEngine.PlayerLoop;

namespace ChatGPT_Detective
{
    public class GPTPromptIntegrator : MonoBehaviour
    {
        private static GPTPromptIntegrator _instance;

        public static GPTPromptIntegrator Instance
        {
            get
            {
                if (!_instance)
                    _instance = FindFirstObjectByType<GPTPromptIntegrator>();

                return _instance;
            }
        }

        private const float WordToTokenRatio = 0.75f;

        static int CountWords(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return 0;
            }
            
            string[] words = input.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            return words.Length;
        }

        static int GetHistoryLength(List<Message> history)
        {
            int length = 0;

            foreach (Message message in history)
            {
                length += CountWords(message.Content);
            }

            return length;
        }

        #region Member Variables

        [SerializeField] private WorldContextInfo _worldContext;

        [SerializeField] [TextArea(5, 15)] private string _baseSystemInstructions = "";
        
        [SerializeField] [TextArea(5, 15)] private string _goalSystemInstructions = "";

        [SerializeField] [Range(0f, 2f)] private float _temperature = 1f;

        [SerializeField] private SystemGoalsManager _goalsManager;

        [SerializeField] private int _modelTPM = 10000;

        [SerializeField] private int _embeddingsWindowSize = 5;

        [SerializeField] private int _embeddingsSampleCount = 10;

        private OpenAIClient _conversationClient;

        private OpenAIClient _embeddingsClient;

        private Action<Message> _onResponseReceived;

        private float _tokenPerSecondRate;
        
        private float _lastMessageTime;

        #endregion

        private void Awake()
        {
            GPTPromptIntegrator[] handlers = FindObjectsByType<GPTPromptIntegrator>(FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            if (handlers.Length > 1)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }

            _conversationClient = new OpenAIClient();

            _embeddingsClient = new OpenAIClient();
        }

        private void Start()
        {
            _tokenPerSecondRate = 1 / (_modelTPM / 60f);

            _lastMessageTime = 0;
        }

        #region OpenAI Functions

        public async void SendPromptMessage(CharacterInfo charInfo, string newPrompt, HistoryData historyData, GoalInfo npcCurrentGoal)
        {
            List<Message> history = await FormatPromptRequest(charInfo.CharInstructions, newPrompt, historyData);

            FormatHistory(charInfo.CharInfo, npcCurrentGoal.Goal, history);

            ChatRequest chatRequest = new ChatRequest(history, Model.GPT4, _temperature);
            ChatResponse response = await _conversationClient.ChatEndpoint.GetCompletionAsync(chatRequest);

            if (response.Choices?.Count > 0)
            {
                Message reply = response.Choices[0].Message;

                _goalsManager.CheckGoalStatus(npcCurrentGoal.Id, history, reply);

                _onResponseReceived?.Invoke(reply);
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }
        }

        public async Task<List<Message>> FormatPromptRequest(string charInstructions, string newPrompt, HistoryData historyData)
        {
            List<Message> validHistory = new List<Message> { new Message(Role.User, "") };

            int historyLength = GetHistoryLength(historyData.HistoryList) + CountWords(newPrompt);

            float passedTime = Time.time - _lastMessageTime;

            if (historyLength > _tokenPerSecondRate * passedTime) 
            {
                EmbeddingsResponse embeddings =
                    await _embeddingsClient.EmbeddingsEndpoint.CreateEmbeddingAsync(newPrompt, Model.Embedding_Ada_002);

                if (embeddings.Data?.Count > 0)
                {
                    IReadOnlyList<double> data = embeddings.Data[0].Embedding;

                    if (historyData.DialogueVectors.Count > 0)
                    {
                        List<DialogueChunk> nearestChunks = historyData.DialogueVectors.FindNearest(data.ToArray(), _embeddingsSampleCount);

                        validHistory.AddRange(GetValidHistory(historyData.PromptHistory, nearestChunks));
                    }

                    validHistory.Add(new Message(Role.User, $"{charInstructions}\n\n###\n\n{newPrompt}"));
                }
                else
                {
                    Debug.LogWarning("No embeddings were generated from this prompt.");
                }
            }
            else
            {

            }

            _lastMessageTime = Time.time;

            return validHistory;
        }

        private List<Message> GetValidHistory(List<DialogueChunk> npcPromptHistory, List<DialogueChunk> nearestChunks)
        {
            List<Message> validHistory = new List<Message>();

            AssignValidDialogues(npcPromptHistory, nearestChunks, validHistory);
            return validHistory;
        }

        private void AssignValidDialogues(List<DialogueChunk> npcPromptHistory, List<DialogueChunk> nearestChunks,
            List<Message> validHistory)
        {
            bool[] addedDialogues = new bool[npcPromptHistory.Count];

            float passedTime = Time.time - _lastMessageTime;

            int availableLength = (int)(_tokenPerSecondRate * passedTime);

            foreach (DialogueChunk chunk in nearestChunks)
            {
                int chunkHistLength = 0;

                List<Message> viableHistory = new List<Message>();

                Vector2Int window = new Vector2Int(chunk.HistoryIndex - _embeddingsWindowSize,
                        chunk.HistoryIndex + _embeddingsWindowSize + 1);

                if (window.x < 0)
                    window.x = 0;

                if (window.y > npcPromptHistory.Count)
                    window.y = npcPromptHistory.Count;

                for (int i = window.x; i < window.y; i++)
                {
                    if (!addedDialogues[i])
                    {
                        DialogueChunk viableChunk = npcPromptHistory[i];

                        viableHistory.Add(viableChunk.Prompt);
                        viableHistory.Add(viableChunk.Response);

                        chunkHistLength += CountWords($"{viableChunk.Prompt} {viableChunk.Response}");
                    }
                }

                if (availableLength > chunkHistLength)
                {
                    validHistory.AddRange(viableHistory);

                    availableLength -= chunkHistLength;
                }
            }
        }

        private void FormatHistory(string npcInfo, string npcCurrentGoal, List<Message> history)
        {
            Message baseSystemMessage = new Message(Role.System,
                $"{_baseSystemInstructions}\n\n###\n\n{_worldContext.GetWorldInfo()}\n\n{npcInfo}");
            
            history[0] = baseSystemMessage;

            Message goalSystemMessage =
                new Message(Role.System, $"{_goalSystemInstructions}\n\n###\n\nGoal: {npcCurrentGoal}");
            
            history.Insert(history.Count - 1, goalSystemMessage);
        }

        #endregion

        #region Delegate Functions

        public void RegisterOnResponseReceived(Action<Message> action)
        {
            _onResponseReceived += action;
        }
        
        public void DeregisterOnResponseReceived(Action<Message> action)
        {
            _onResponseReceived -= action;
        }

        #endregion
    }
}
