using OpenAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChatGPT_Detective
{
    public class NPCPromptHandler : MonoBehaviour
    {
        [SerializeField] private CharacterInfo _characterInfo;

        [SerializeField] private int _contextWindowSize = 5;
        [SerializeField] private int _resultSampleCount = 3;

        private List<ChatMessage> _npcMessageHistory = new List<ChatMessage>();

        private void ProcessPromptRequest(string newMessage)
        {
            List<ChatMessage> messages = new List<ChatMessage>();
            messages.Add(new ChatMessage());

            //OpenAIApi openai = new OpenAIApi();
            //openai.CreateEmbeddings()
        }
    }
}
