using System;
using OpenAI;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using OpenAI.Chat;
using OpenAI.Models;
using OpenAI.Embeddings;
using System.Linq;

namespace ChatGPT_Detective
{
    public class GPTPromptIntegrator : MonoBehaviour
    {
        private static GPTPromptIntegrator s_instance;

        public static GPTPromptIntegrator Instance
        {
            get
            {
                if (!s_instance)
                    s_instance = FindFirstObjectByType<GPTPromptIntegrator>();

                return s_instance;
            }
        }

        private const float k_WordToTokenRatio = 0.75f;

        private static int CountWords(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return 0;
            }

            string[] words = input.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            return words.Length;
        }

        private static int GetHistoryLength(List<Message> history)
        {
            int length = 0;

            foreach (Message message in history)
            {
                length += CountWords(message.Content);
            }

            return length;
        }
        
        [SerializeField] private WorldContextInfo m_worldContext;

        [SerializeField][TextArea(5, 15)] private string m_baseSystemInstructions = "";

        [SerializeField][TextArea(5, 15)] private string m_goalSystemInstructions = "";

        [SerializeField][Range(0f, 2f)] private float m_temperature = 1f;

        [SerializeField] private SystemGoalsManager m_goalsManager;

        [SerializeField] private int m_modelTPM = 10000;

        [SerializeField] private int m_embeddingsWindowSize = 5;

        [SerializeField] private int m_embeddingsSampleCount = 10;

        private OpenAIClient m_conversationClient;

        private OpenAIClient m_embeddingsClient;

        private Action<string> m_onResponseStreaming;

        private Action<Message> m_onResponseReceived;

        private Queue<PromptMessageData> m_promptQueue = new Queue<PromptMessageData>();

        private bool m_goalCheckedForLastMessage = true;

        private bool m_processingPrompt;

        private float m_tokenPerSecondRate;

        private float m_lastMessageTime;
        
        private void Awake()
        {
            GPTPromptIntegrator[] integrators = FindObjectsByType<GPTPromptIntegrator>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            if (integrators.Length > 1)
            {
                Destroy(gameObject);
            }
            else
            {
                s_instance = this;
            }

            m_conversationClient = new OpenAIClient();

            m_embeddingsClient = new OpenAIClient();
        }
        
        private void Start()
        {
            m_tokenPerSecondRate = 1 / (m_modelTPM / 60f);

            m_lastMessageTime = 0;
        }

        private void Update()
        {
            if (m_promptQueue.Count > 0 && m_goalCheckedForLastMessage && !m_processingPrompt)
            {
                PromptMessageData prompt = m_promptQueue.Dequeue();

                SendPromptMessage(prompt);
            }
        }

        public async void SendPromptMessage(PromptMessageData promptMessage)
        {
            if (m_goalCheckedForLastMessage && !m_processingPrompt)
            {
                m_processingPrompt = true;

                List<Message> history = await FormatPromptRequest(promptMessage.CharInfo.CharInstructions,
                    promptMessage.NewPrompt,
                    promptMessage.HistoryData);

                FormatHistory(promptMessage.CharInfo.CharInfo, promptMessage.NpcCurrentGoal.Goal, history);

                ChatRequest chatRequest = new ChatRequest(history, Model.GPT4, m_temperature);

                await m_conversationClient.ChatEndpoint.StreamCompletionAsync(chatRequest, async response =>
                {
                    if (response.Choices?.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(response.Choices[0].Delta?.Content))
                        {
                            m_onResponseStreaming.Invoke(response.Choices[0].Delta.Content);
                        }

                        if (!string.IsNullOrEmpty(response.Choices[0].Message?.Content))
                        {
                            Message reply = response.Choices[0].Message;

                            m_processingPrompt = false;
                            m_goalCheckedForLastMessage = false;

                            m_onResponseReceived?.Invoke(reply);
                            
                            m_goalCheckedForLastMessage =
                                await m_goalsManager.CheckGoalStatus(promptMessage.NpcCurrentGoal.Id, history, reply);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("No text was generated from this prompt.");
                    }
                });
            }
            else
            {
                m_promptQueue.Enqueue(promptMessage);
            }
        }

        public async Task<List<Message>> FormatPromptRequest(string charInstructions, string newPrompt, HistoryData historyData)
        {
            List<Message> validHistory = new List<Message> { new Message(Role.User, "") };

            int historyLength = GetHistoryLength(historyData.HistoryList) + CountWords(newPrompt);

            float passedTime = Time.time - m_lastMessageTime;

            if (historyLength > m_tokenPerSecondRate * passedTime)
            {
                EmbeddingsResponse embeddings =
                    await m_embeddingsClient.EmbeddingsEndpoint.CreateEmbeddingAsync(newPrompt, Model.Embedding_Ada_002);

                if (embeddings.Data?.Count > 0)
                {
                    IReadOnlyList<double> data = embeddings.Data[0].Embedding;

                    if (historyData.DialogueVectors.Count > 0)
                    {
                        List<DialogueChunk> nearestChunks = historyData.DialogueVectors.FindNearest(data.ToArray(),
                            m_embeddingsSampleCount);

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
                validHistory.AddRange(historyData.HistoryList);
            }

            m_lastMessageTime = Time.time;

            return validHistory;
        }

        private async void HandleResponse(ChatResponse response)
        {

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

            float passedTime = Time.time - m_lastMessageTime;

            int availableLength = (int)(m_tokenPerSecondRate * passedTime);

            foreach (DialogueChunk chunk in nearestChunks)
            {
                int chunkHistLength = 0;

                List<Message> viableHistory = new List<Message>();

                Vector2Int window = new Vector2Int(chunk.HistoryIndex - m_embeddingsWindowSize,
                    chunk.HistoryIndex + m_embeddingsWindowSize + 1);

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
                $"{m_baseSystemInstructions}\n\n###\n\n{m_worldContext.GetWorldInfo()}\n\n{npcInfo}");

            history[0] = baseSystemMessage;

            Message goalSystemMessage =
                new Message(Role.System, $"{m_goalSystemInstructions}\n\n###\n\nGoal: {npcCurrentGoal}");

            history.Insert(history.Count - 1, goalSystemMessage);
        }

        private void ProcessGoalCheck()
        {
            m_goalCheckedForLastMessage = true;
        }
        
        public void RegisterOnResponseReceived(Action<Message> action)
        {
            m_onResponseReceived += action;
        }

        public void RegisterOnResponseStreaming(Action<string> action)
        {
            m_onResponseStreaming += action;
        }

        public void DeregisterOnResponseReceived(Action<Message> action)
        {
            m_onResponseReceived -= action;
        }

        public void DeregisterOnResponseStreaming(Action<string> action)
        {
            m_onResponseStreaming -= action;
        }
    }
}
