using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChatGPT_Detective
{
    [System.Serializable]
    public struct SerializableGoalLog
    {
        public int mGoalId;

        public int mGoalLog;

        public SerializableGoalLog(KeyValuePair<int, int> log)
        {
            mGoalId = log.Key;
            mGoalLog = log.Value;
        }
    }

    [System.Serializable]
    public class GoalsSaveData
    {
        public SerializableGoalLog[] mGoalLogs;

        public GoalsSaveData(IReadOnlyDictionary<int, int> goalLogs)
        {
            mGoalLogs = new SerializableGoalLog[goalLogs.Count];

            int i = 0;
            foreach (KeyValuePair<int, int> log in goalLogs)
            {
                mGoalLogs[i++] = new SerializableGoalLog(log);
            }
        }
    }
}