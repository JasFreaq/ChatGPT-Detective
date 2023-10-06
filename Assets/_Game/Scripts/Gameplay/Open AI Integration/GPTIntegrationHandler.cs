using System;
using OpenAI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

namespace ChatGPT_Detective
{
    public class GPTIntegrationHandler : MonoBehaviour
    {
        private static GPTIntegrationHandler _instance;

        public static GPTIntegrationHandler Instance
        {
            get { return _instance; } 
        }

        #region Member Variables

        [SerializeField] private WorldContextInfo _worldContext;

        [SerializeField] [TextArea(5, 15)] private string _mainPromptInstructions = "";

        private OpenAIApi _openAi = new OpenAIApi();

        private Action<ChatMessage> _onResponseReceived;

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
        }

        #region OpenAI Functions
        
        public async void SendPromptMessage(string npcInstructions, List<ChatMessage> history)
        {
            ChatMessage tempMessage = new ChatMessage()
            {
                Role = "system",
                Content = _mainPromptInstructions + $"\n\n{npcInstructions}"
            };
            history[0] = tempMessage;

            CreateChatCompletionResponse completionResponse = await _openAi.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo",
                Messages = history
            });

            if (completionResponse.Choices?.Count > 0)
            {
                ChatMessage response = completionResponse.Choices[0].Message;
                response.Content = response.Content.Trim();

                _onResponseReceived?.Invoke(response);
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }
        }

        #endregion

        #region Delegate Functions

        public void RegisterOnResponseReceived(Action<ChatMessage> action)
        {
            _onResponseReceived += action;
        }
        
        public void DeregisterOnResponseReceived(Action<ChatMessage> action)
        {
            _onResponseReceived -= action;
        }

        #endregion
    }
}
