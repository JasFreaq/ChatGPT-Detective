using System;
using OpenAI;
using System.Collections.Generic;
using System.Linq;
using OpenAI.Chat;
using OpenAI.Embeddings;
using OpenAI.Models;
using UnityEngine;

namespace ChatGPT_Detective
{
    public class NpcPrompter : MonoBehaviour
    {
        public class NpcPrompterComparer : IComparer<NpcPrompter>
        {
            public int Compare(NpcPrompter a, NpcPrompter b)
            {
                return a.m_charInfo.CharId.CompareTo(b.m_charInfo.CharId);
            }
        }

        [SerializeField] private CharacterInfo m_charInfo;

        private HistoryData m_historyData = new HistoryData();

        private OpenAIClient m_embeddingsClient;

        private NpcGoalsHandler m_goalsHandler;

        private string m_lastPrompt;

        private bool m_sentPrompt;

        public CharacterInfo CharInfo => m_charInfo;

        public IReadOnlyList<DialogueChunk> History => m_historyData.PromptHistory;
        
        private void Awake()
        {
            m_embeddingsClient = new OpenAIClient();

            m_goalsHandler = GetComponent<NpcGoalsHandler>();
        }

        private void Start()
        {
            m_goalsHandler.SetupGoalHandling(m_charInfo.CharGoals, m_charInfo.CharFallbackGoal);
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
            PromptMessageData promptMessage = new PromptMessageData(m_charInfo, newPrompt, m_historyData, m_goalsHandler.CurrentGoal);

            GPTPromptIntegrator.Instance.SendPromptMessage(promptMessage);

            m_lastPrompt = newPrompt;

            m_sentPrompt = true;
        }

        private void UpdateHistory(Message response)
        {
            if (m_sentPrompt) 
            {
                UpdateHistoryAsync(response);
                m_sentPrompt = false;
            }
        }

        private async void UpdateHistoryAsync(Message response)
        {
            EmbeddingsResponse embeddings =
                await m_embeddingsClient.EmbeddingsEndpoint.CreateEmbeddingAsync($"{m_lastPrompt}\n{response.Content}",
                    Model.Embedding_Ada_002);

            if (embeddings.Data?.Count > 0)
            {
                Message lastMessage = new Message(Role.User, m_lastPrompt);

                DialogueChunk newChunk = new DialogueChunk(m_historyData.PromptHistory.Count,
                    lastMessage, response,
                    embeddings.Data[0].Embedding.ToArray());

                m_historyData.Add(newChunk);
            }
            else
            {
                Debug.LogWarning("No embeddings were generated from this prompt.");
            }
        }

        public void InitialiseFromSaveData(SerializableDialogueChunk[] promptHistory)
        {
            foreach (SerializableDialogueChunk serializableChunk in promptHistory)
            {
                m_historyData = new HistoryData();

                DialogueChunk newChunk = new DialogueChunk(serializableChunk);

                m_historyData.Add(newChunk);
            }
        }
    }
}
