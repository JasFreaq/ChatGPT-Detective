using System;
using System.Collections;
using System.Collections.Generic;
using OpenAI;
using OpenAI.Chat;
using UnityEngine;

namespace ChatGPT_Detective
{
    public class DialogueChunk : IVectorObject
    {
        private int _historyIndex;

        public double[]? _messageVectors;

        private Message _prompt;
        private Message _response;

        public DialogueChunk(int index, Message promptMsg, Message responseMsg, double[] vectors)
        {
            _historyIndex = index;
            _prompt = promptMsg;
            _response = responseMsg;
            _messageVectors = vectors;
        }

        public int HistoryIndex => _historyIndex;
        
        public Message Prompt => _prompt;

        public Message Response => _response;

        public double[] GetVector()
        {
            return _messageVectors ?? throw new Exception("Message Vectors not set");
        }
    }
}