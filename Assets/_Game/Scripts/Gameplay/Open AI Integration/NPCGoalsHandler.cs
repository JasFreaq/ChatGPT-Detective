using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChatGPT_Detective
{
    public class NpcGoalsHandler : MonoBehaviour
    {
        private IReadOnlyList<GoalInfo> m_goalsList = new List<GoalInfo>();

        private Dictionary<int, bool> m_goalClearingTracker = new Dictionary<int, bool>();
        
        private GoalInfo m_currentGoal;

        private GoalInfo m_fallbackGoal;

        private int m_currentGoalIndex;

        public GoalInfo CurrentGoal => m_currentGoal;

        public void SetupGoalHandling(IReadOnlyList<GoalInfo> charGoals, string charFallbackGoal)
        {
            m_goalsList = charGoals;
            m_currentGoal = m_goalsList[0];
            m_fallbackGoal = new GoalInfo(0, charFallbackGoal);

            foreach (GoalInfo goal in m_goalsList)
            {
                foreach (int prereqId in goal.PrerequisiteIds)
                {
                    m_goalClearingTracker.Add(prereqId, false);
                }
            }
        }

        public void UpdateGoals(int clearedId)
        {
            if (m_goalClearingTracker.TryGetValue(clearedId, out bool value))
            {
                m_goalClearingTracker[clearedId] = true;

                if (m_currentGoalIndex < m_goalsList.Count - 1) 
                {
                    GoalInfo nextGoal = m_goalsList[m_currentGoalIndex + 1];

                    bool clearedNextGoalPrerequisites = true;
                    foreach (int id in nextGoal.PrerequisiteIds)
                    {
                        clearedNextGoalPrerequisites &= m_goalClearingTracker[id];
                    }

                    if (clearedNextGoalPrerequisites)
                    {
                        m_currentGoalIndex++;
                        m_currentGoal = m_goalsList[m_currentGoalIndex];
                    }
                    else
                    {
                        m_currentGoal = m_fallbackGoal;
                    }
                }
                else
                {
                    m_currentGoal = m_fallbackGoal;
                }
            }
        }
    }
}