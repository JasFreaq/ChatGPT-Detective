using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ChatGPT_Detective
{
    public class GoalEventsHandler : MonoBehaviour
    {
        [System.Serializable]
        private struct GoalEvent
        {
            public int mGoalId;

            public UnityEvent mGoalEvent;
        }

        [SerializeField] private List<GoalEvent> m_goalEvents = new List<GoalEvent>();
        
        public void CheckGoalEvents(int clearedId)
        {
            foreach (GoalEvent goalEvent in m_goalEvents)
            {
                if (goalEvent.mGoalId == clearedId)
                {
                    goalEvent.mGoalEvent?.Invoke();
                }
            }
        }
    }
}