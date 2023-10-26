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

        private double[]? _messageVectors;

        private Message _prompt;
        private Message _response;

        public DialogueChunk(int index, Message promptMsg, Message responseMsg, double[] vectors)
        {
            _historyIndex = index;
            _prompt = promptMsg;
            _response = responseMsg;
            _messageVectors = vectors;
        }
        
        public DialogueChunk(SerializableDialogueChunk serializableChunk)
        {
            _historyIndex = serializableChunk.historyIndex;
            _prompt = GetMessageFromSerializable(serializableChunk.prompt);
            _response = GetMessageFromSerializable(serializableChunk.response);
            _messageVectors = serializableChunk.messageVectors;
        }

        public int HistoryIndex => _historyIndex;
        
        public Message Prompt => _prompt;

        public Message Response => _response;

        public double[] GetVector()
        {
            return _messageVectors ?? throw new Exception("Message Vectors not set");
        }

        private static Message GetMessageFromSerializable(SerializableMessage serializableMessage)
        {
            Role role = Enum.Parse<Role>(serializableMessage.role);

            Message message = new Message(role, serializableMessage.content, serializableMessage.name);
            return message;
        }
    }
}