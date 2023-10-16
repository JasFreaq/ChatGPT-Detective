using ChatGPT_Detective;
using System.Collections;
using System.Collections.Generic;
using OpenAI.Chat;
using Unity.VisualScripting;
using UnityEngine;

public class HistoryData
{
    private const int VectorDimension = 1536;

    private List<DialogueChunk> _promptHistory;

    private VectorCollection<DialogueChunk> _dialogueVectors;

    public List<DialogueChunk> PromptHistory => _promptHistory;

    public VectorCollection<DialogueChunk> DialogueVectors => _dialogueVectors;

    public List<Message> HistoryList
    {
        get
        {
            List<Message> history = new List<Message>();

            foreach (DialogueChunk chunk in _promptHistory)
            {
                history.Add(chunk.Prompt);
                history.Add(chunk.Response);
            }

            return history;
        }
    }

    public HistoryData()
    {
        _promptHistory = new List<DialogueChunk>();

        _dialogueVectors = new VectorCollection<DialogueChunk>(VectorDimension);
    }

    public void Add(DialogueChunk chunk)
    {
        _promptHistory.Add(chunk);
        _dialogueVectors.Add(chunk);
    }
}
