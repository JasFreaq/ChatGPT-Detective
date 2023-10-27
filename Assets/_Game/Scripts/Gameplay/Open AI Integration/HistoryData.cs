using System.Collections.Generic;
using OpenAI.Chat;

namespace ChatGPT_Detective
{
    public class HistoryData
    {
        private List<DialogueChunk> m_promptHistory;

        private VectorCollection<DialogueChunk> m_dialogueVectors;

        public List<DialogueChunk> PromptHistory => m_promptHistory;

        public VectorCollection<DialogueChunk> DialogueVectors => m_dialogueVectors;

        public List<Message> HistoryList
        {
            get
            {
                List<Message> history = new List<Message>();

                foreach (DialogueChunk chunk in m_promptHistory)
                {
                    history.Add(chunk.Prompt);
                    history.Add(chunk.Response);
                }

                return history;
            }
        }

        public HistoryData()
        {
            m_promptHistory = new List<DialogueChunk>();
            m_dialogueVectors = new VectorCollection<DialogueChunk>();
        }

        public void Add(DialogueChunk chunk)
        {
            m_promptHistory.Add(chunk);
            m_dialogueVectors.Add(chunk);
        }
    }
}