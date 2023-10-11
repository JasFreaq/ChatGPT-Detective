using System;
using OpenAI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using System.Data;
using OpenAI.Chat;
using OpenAI.Models;

namespace ChatGPT_Detective
{
    public class GPTIntegrationHandler : MonoBehaviour
    {
        private static GPTIntegrationHandler _instance;

        public static GPTIntegrationHandler Instance
        {
            get
            {
                if (!_instance)
                    _instance = FindObjectOfType<GPTIntegrationHandler>();

                return _instance;
            }
        }

        #region Member Variables

        [SerializeField] private WorldContextInfo _worldContext;

        [SerializeField] [TextArea(5, 15)] private string _baseSystemInstructions = "";
        
        [SerializeField] [TextArea(5, 15)] private string _goalSystemInstructions = "";

        [SerializeField] [Range(0f, 2f)] private float _temperature = 1f;

        private OpenAIClient _openAi;

        private Action<Message> _onResponseReceived;

        #endregion
        
        private void Awake()
        {
            GPTIntegrationHandler[] handlers = FindObjectsByType<GPTIntegrationHandler>(FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            if (handlers.Length > 1)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }

            _openAi = new OpenAIClient();
        }

        #region OpenAI Functions
        
        public async void SendPromptMessage(string npcInfo, string npcCurrentGoal, List<Message> history)
        {
            FormatHistory(npcInfo, npcCurrentGoal, history);
            
            ChatRequest chatRequest = new ChatRequest(history, Model.GPT4, _temperature);
            ChatResponse response = await _openAi.ChatEndpoint.GetCompletionAsync(chatRequest);

            if (response.Choices?.Count > 0)
            {
                Message reply = response.Choices[0].Message;

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
