using System;
using OpenAI;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI.Chat;
using Newtonsoft.Json;
using UnityEngine;
using Newtonsoft.Json.Linq;
using OpenAI.Models;
using Unity.VisualScripting.Dependencies.NCalc;
using System.Threading;

namespace ChatGPT_Detective
{
    public class SystemGoalsManager : MonoBehaviour
    {
        private class GoalStatusArgs
        {
            public bool mStatus;
        }

        [SerializeField] private int m_attemptsThresholdBeforeGoalClears = 3;

        [SerializeField] private int m_goalCheckWaitTime = 20;

        [SerializeField] private List<NpcGoalsHandler> m_npcGoalsHandlers = new List<NpcGoalsHandler>();

        private OpenAIClient m_goalClient;
        
        private GoalEventsHandler m_goalEventsHandler;
        
        private List<Function> m_goalFunction = new List<Function>
        {
            new Function("CheckGoalStatus", "Tells the user whether the goal was completed or not.",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["mStatus"] = new JObject
                        {
                            ["type"] = "boolean",
                            ["description"] = "The goal completion status."
                        }
                    },
                    ["required"] = new JArray { "mStatus" }
                })
        };

        private Dictionary<int, int> m_clearedGoalsLog = new Dictionary<int, int>();

        public IReadOnlyDictionary<int, int> ClearedGoalsLog => m_clearedGoalsLog;

        private void Awake()
        {
            m_goalClient = new OpenAIClient();

            m_goalEventsHandler = GetComponent<GoalEventsHandler>();
        }

        public async Task<bool> CheckGoalStatus(int goalId, List<Message> history, Message reply)
        {
            if (goalId != 0)
            {
                if (m_clearedGoalsLog.ContainsKey(goalId))
                {
                    m_clearedGoalsLog[goalId]++;
                }
                else
                {
                    m_clearedGoalsLog.Add(goalId, 0);
                }

                history.Add(reply);
                
                ChatRequest goalRequest =
                    new ChatRequest(history, Model.GPT3_5_Turbo, 0f, functions: m_goalFunction, functionCall: "CheckGoalStatus");

                await CheckGoalWithTimeout(goalId, goalRequest);
            }

            return true;
        }

        private async Task CheckGoalWithTimeout(int goalId, ChatRequest goalRequest)
        {
            int timeoutMilliseconds = m_goalCheckWaitTime * 1000;
            Task<ChatResponse> task = m_goalClient.ChatEndpoint.GetCompletionAsync(goalRequest);

            Task delayTask = Task.Delay(timeoutMilliseconds);
            Task completedTask = await Task.WhenAny(task, delayTask);

            if (completedTask == delayTask)
            {
                UpdateGoalStatus(false, goalId);
            }
            else
            {
                ChatResponse goalResponse = await task;

                GoalStatusArgs functionArgs =
                    JsonConvert.DeserializeObject<GoalStatusArgs>(goalResponse.FirstChoice.Message.Function.Arguments
                        .ToString());

                UpdateGoalStatus(functionArgs.mStatus, goalId);
            }
        }

        private void UpdateGoalStatus(bool status, int id)
        {
            if (m_clearedGoalsLog.ContainsKey(id))
            {
                m_clearedGoalsLog[id]++;
            }
            else
            {
                m_clearedGoalsLog.Add(id, 0);
            }

            if (status || m_clearedGoalsLog[id] >= m_attemptsThresholdBeforeGoalClears)
            {
                m_clearedGoalsLog[id] = m_attemptsThresholdBeforeGoalClears;

                foreach (NpcGoalsHandler goalsHandler in m_npcGoalsHandlers)
                {
                    goalsHandler.UpdateGoals(id, m_clearedGoalsLog, m_attemptsThresholdBeforeGoalClears);
                }

                m_goalEventsHandler.CheckGoalEvents(id);
            }
        }

        public void LoadGoalLog(GoalsSaveData goalsSave)
        {
            m_clearedGoalsLog = new Dictionary<int, int>();

            for (int i = 0, l = goalsSave.mGoalLogs.Length; i < l; i++)
            {
                m_clearedGoalsLog.Add(goalsSave.mGoalLogs[i].mGoalId, goalsSave.mGoalLogs[i].mGoalLog);
            }
        }
    }
}
