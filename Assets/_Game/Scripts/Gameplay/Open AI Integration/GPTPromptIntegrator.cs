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

        #region Member Variables

        [SerializeField] private WorldContextInfo _worldContext;

        [SerializeField] [TextArea(5, 15)] private string _baseSystemInstructions = "";
        
        [SerializeField] [TextArea(5, 15)] private string _goalSystemInstructions = "";

        [SerializeField] [Range(0f, 2f)] private float _temperature = 1f;

        [SerializeField] private SystemGoalsManager _goalsManager;

        private OpenAIClient _conversationClient;
        
        private Action<Message> _onResponseReceived;

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
        }

        #region OpenAI Functions
        
        public async void SendPromptMessage(int goalId, string npcInfo, string npcCurrentGoal, List<Message> history)
        {
            FormatHistory(npcInfo, npcCurrentGoal, history);
            
            ChatRequest chatRequest = new ChatRequest(history, Model.GPT4, _temperature);
            ChatResponse response = await _conversationClient.ChatEndpoint.GetCompletionAsync(chatRequest);

            if (response.Choices?.Count > 0)
            {
                Message reply = response.Choices[0].Message;

                _goalsManager.CheckGoalStatus(goalId, history, reply);

                _onResponseReceived?.Invoke(reply);
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
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
