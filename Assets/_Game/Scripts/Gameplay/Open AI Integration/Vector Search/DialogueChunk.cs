using System;
using System.Collections;
using System.Collections.Generic;
using OpenAI;
using UnityEngine;

namespace ChatGPT_Detective
{
    public class DialogueChunk : MonoBehaviour, IVectorObject
    {
        private int _historyIndex;

        public float[]? _messageVectors;

        private ChatMessage _prompt;
        private ChatMessage _response;

        public DialogueChunk(int index, ChatMessage promptMsg, ChatMessage responseMsg, float[] vectors)
        {
            _historyIndex = index;
            _prompt = promptMsg;
            _response = responseMsg;
            _messageVectors = vectors;
        }

        public int HistoryIndex => _historyIndex;
        
        public ChatMessage Prompt => _prompt;

        public ChatMessage Response => _response;

        public float[] GetVector()
        {
            return _messageVectors ?? throw new Exception("Message Vectors not set");
        }
    }
}