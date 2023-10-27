using ChatGPT_Detective;
using System.Collections.Generic;
using OpenAI.Chat;

namespace ChatGPT_Detective
{
    [System.Serializable]
    public struct SerializableMessage
    {
        public string mContent;

        public string mName;

        public string mRole;

        public SerializableMessage(Message message)
        {
            mContent = message.Content;
            mName = message.Name;
            mRole = message.Role.ToString();
        }
    }


    public struct SerializableDialogueChunk
    {
        public int mHistoryIndex;

        public double[]? mMessageVectors;

        public SerializableMessage mPrompt;
        public SerializableMessage mResponse;

        public SerializableDialogueChunk(DialogueChunk chunk)
        {
            mHistoryIndex = chunk.HistoryIndex;

            mMessageVectors = chunk.GetVector();

            mPrompt = new SerializableMessage(chunk.Prompt);
            mResponse = new SerializableMessage(chunk.Response);
        }
    }

    [System.Serializable]
    public class NpcHistorySaveData
    {
        public int mCharId;

        public List<SerializableDialogueChunk> mPromptHistory;

        public NpcHistorySaveData(NpcPrompter npc)
        {
            mCharId = npc.CharInfo.CharId;

            mPromptHistory = new List<SerializableDialogueChunk>();
            foreach (DialogueChunk chunk in npc.History)
            {
                mPromptHistory.Add(new SerializableDialogueChunk(chunk));
            }
        }
    }
}