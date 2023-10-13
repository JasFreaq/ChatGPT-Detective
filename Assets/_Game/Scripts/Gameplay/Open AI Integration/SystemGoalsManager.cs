using System;
using OpenAI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatGPT_Detective;
using Newtonsoft.Json.Linq;
using OpenAI.Chat;
using UnityEngine;
using Unity.Mathematics;
using Newtonsoft.Json;

public class SystemGoalsManager : MonoBehaviour
{
    private class GoalStatusArgs
    {
        public bool status;
    }

    [SerializeField] private List<NpcGoalsHandler> _npcGoalsHandlers = new List<NpcGoalsHandler>();
    
    private OpenAIClient _goalClient;
    
    private List<Function> _goalFunction = new List<Function>
    {
        new Function("CheckGoalStatus", "Tells the user whether the goal was completed or not.",
            new JObject
            {
                ["type"] = "object",
                ["properties"] = new JObject
                {
                    ["status"] = new JObject
                    {
                        ["type"] = "boolean",
                        ["description"] = "The goal completion status."
                    }
                },
                ["required"] = new JArray { "status" }
            })
    };

    private void Awake()
    {
        _goalClient = new OpenAIClient();
    }
    
    public async void CheckGoalStatus(int goalId, List<Message> history, Message reply)
    {
        history.Add(reply);

        ChatRequest goalRequest =
            new ChatRequest(history, functions: _goalFunction, functionCall: "CheckGoalStatus", model: "gpt-3.5-turbo");

        ChatResponse goalResponse = await _goalClient.ChatEndpoint.GetCompletionAsync(goalRequest);

        GoalStatusArgs functionArgs = JsonConvert.DeserializeObject<GoalStatusArgs>(goalResponse.FirstChoice.Message.Function.Arguments.ToString());
        
        if (functionArgs.status)
        {
            UpdateGoalStatus(goalId);
        }
    }

    private void UpdateGoalStatus(int id)
    {
        foreach (NpcGoalsHandler goalsHandler in _npcGoalsHandlers)
        {
            goalsHandler.UpdateGoals(id);
        }
    }
}
