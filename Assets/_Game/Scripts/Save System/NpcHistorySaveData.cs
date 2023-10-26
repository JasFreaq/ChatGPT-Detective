using ChatGPT_Detective;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using OpenAI.Chat;
using UnityEngine;

[System.Serializable]
public struct SerializableMessage
{
    public string content;

    public string name;

    public string role;

    public SerializableMessage(Message message)
    {
        content = message.Content;
        name = message.Name;
        role = message.Role.ToString();
    }
}


public struct SerializableDialogueChunk
{
    public int historyIndex;

    public double[]? messageVectors;

    public SerializableMessage prompt;
    public SerializableMessage response;

    public SerializableDialogueChunk(DialogueChunk chunk)
    {
        historyIndex = chunk.HistoryIndex;

        messageVectors = chunk.GetVector();

        prompt = new SerializableMessage(chunk.Prompt);
        response = new SerializableMessage(chunk.Response);
    }
}

[System.Serializable]
public class NpcHistorySaveData
{
    public int charId;

    public List<SerializableDialogueChunk> promptHistory;

    public NpcHistorySaveData(NpcPrompter npc)
    {
        charId = npc.CharInfo.CharId;

        promptHistory = new List<SerializableDialogueChunk>();
        foreach (DialogueChunk chunk in npc.History)
        {
            promptHistory.Add(new SerializableDialogueChunk(chunk));
        }
    }
}
