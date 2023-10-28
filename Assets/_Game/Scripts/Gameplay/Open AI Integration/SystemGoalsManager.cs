using System;
using OpenAI;
using System.Collections.Generic;
using OpenAI.Chat;
using Newtonsoft.Json;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace ChatGPT_Detective
{
    public class SystemGoalsManager : MonoBehaviour
    {
        private class GoalStatusArgs
        {
            public bool m_status;
        }

        [SerializeField] private List<NpcGoalsHandler> m_npcGoalsHandlers = new List<NpcGoalsHandler>();

        private GoalEventsHandler m_goalEventsHandler;

        private OpenAIClient m_goalClient;

        private List<Function> m_goalFunction = new List<Function>
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

        private Action m_onGoalChecked;

        private void Awake()
        {
            m_goalClient = new OpenAIClient();

            m_goalEventsHandler = GetComponent<GoalEventsHandler>();
        }

        public async void CheckGoalStatus(int goalId, List<Message> history, Message reply)
        {
            if (goalId != 0) 
            {
                history.Add(reply);

                ChatRequest goalRequest =
                    new ChatRequest(history, functions: m_goalFunction, functionCall: "CheckGoalStatus",
                        model: "gpt-3.5-turbo");

                ChatResponse goalResponse = await m_goalClient.ChatEndpoint.GetCompletionAsync(goalRequest);

                GoalStatusArgs functionArgs =
                    JsonConvert.DeserializeObject<GoalStatusArgs>(goalResponse.FirstChoice.Message.Function.Arguments
                        .ToString());
                Debug.Log(functionArgs.m_status);
                if (functionArgs.m_status)
                {
                    UpdateGoalStatus(goalId);
                }

                m_onGoalChecked?.Invoke();
            }
        }

        private void UpdateGoalStatus(int id)
        {
            foreach (NpcGoalsHandler goalsHandler in m_npcGoalsHandlers)
            {
                goalsHandler.UpdateGoals(id);
            }

            m_goalEventsHandler.CheckGoalEvents(id);
        }

        public void RegisterOnGoalChecked(Action action)
        {
            m_onGoalChecked += action;
        }

        public void DeregisterOnGoalChecked(Action action)
        {
            m_onGoalChecked -= action;
        }
    }
}
