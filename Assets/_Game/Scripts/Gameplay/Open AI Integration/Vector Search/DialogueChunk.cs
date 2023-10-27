using System;
using OpenAI.Chat;

namespace ChatGPT_Detective
{
    public class DialogueChunk : IVectorObject
    {
        private int m_historyIndex;

        private double[]? m_messageVectors;

        private Message m_prompt;

        private Message m_response;

        public int HistoryIndex => m_historyIndex;

        public Message Prompt => m_prompt;

        public Message Response => m_response;

        public DialogueChunk(int index, Message promptMsg, Message responseMsg, double[] vectors)
        {
            m_historyIndex = index;
            m_prompt = promptMsg;
            m_response = responseMsg;
            m_messageVectors = vectors;
        }

        public DialogueChunk(SerializableDialogueChunk serializableChunk)
        {
            m_historyIndex = serializableChunk.mHistoryIndex;
            m_prompt = GetMessageFromSerializable(serializableChunk.mPrompt);
            m_response = GetMessageFromSerializable(serializableChunk.mResponse);
            m_messageVectors = serializableChunk.mMessageVectors;
        }

        public double[] GetVector()
        {
            return m_messageVectors ?? throw new Exception("Message Vectors not set");
        }

        private static Message GetMessageFromSerializable(SerializableMessage serializableMessage)
        {
            Role role = Enum.Parse<Role>(serializableMessage.mRole);

            Message message = new Message(role, serializableMessage.mContent, serializableMessage.mName);
            return message;
        }
    }
}