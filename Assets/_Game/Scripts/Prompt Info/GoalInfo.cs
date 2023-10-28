using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ChatGPT_Detective
{
    [System.Serializable]
    public class GoalInfo
    {
        [SerializeField] private int m_id;

        [SerializeField] private List<int> m_prerequisiteIds;

        [SerializeField][TextArea(3, 10)] private string m_goal;

        public int Id => m_id;

        public IReadOnlyList<int> PrerequisiteIds => m_prerequisiteIds;

        public string Goal => m_goal;

        public GoalInfo(int id, GoalInfo goalInfo)
        {
            m_id = id;

            m_prerequisiteIds = (List<int>)goalInfo.PrerequisiteIds;
            m_goal = goalInfo.Goal;
        }
        
        public GoalInfo(int id, string goal)
        {
            m_id = id;

            m_prerequisiteIds = null;
            m_goal = goal;
        }
    }
}