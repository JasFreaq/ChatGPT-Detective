using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChatGPT_Detective
{
    public class NpcGoalsHandler : MonoBehaviour
    {
        private IReadOnlyList<GoalInfo> m_goalsList = new List<GoalInfo>();
        
        private GoalInfo m_currentGoal;

        private GoalInfo m_fallbackGoal;

        private int m_currentGoalIndex;

        private bool m_isUsingFallbackGoal;

        public GoalInfo CurrentGoal => m_currentGoal;

        public int CurrentGoalIndex => m_currentGoalIndex;

        public bool IsUsingFallbackGoal => m_isUsingFallbackGoal;

        public void InitializeGoalHandling(IReadOnlyList<GoalInfo> charGoals, string charFallbackGoal)
        {
            m_goalsList = charGoals;
            m_currentGoal = m_goalsList[0];
            m_fallbackGoal = new GoalInfo(0, charFallbackGoal);
        }
        
        public void LoadGoalSave(NpcGoalSaveData goalSave)
        {
            m_currentGoalIndex = goalSave.mNpcGoalIndex;
            m_isUsingFallbackGoal = goalSave.mWasUsingFallbackGoal;

            m_currentGoal = m_isUsingFallbackGoal ? m_fallbackGoal : m_goalsList[m_currentGoalIndex];
        }
        
        public void UpdateGoals(int clearedId, Dictionary<int, int> clearedGoalsLog, int threshold)
        {
            if (clearedId == m_currentGoal.Id)
            {
                if (m_currentGoalIndex < m_goalsList.Count - 1)
                {
                    GoalInfo nextGoal = m_goalsList[m_currentGoalIndex + 1];

                    bool clearedNextGoalPrerequisites = true;
                    foreach (int id in nextGoal.PrerequisiteIds)
                    {
                        if (clearedGoalsLog.ContainsKey(id))
                        {
                            clearedNextGoalPrerequisites &= clearedGoalsLog[id] >= threshold;
                        }
                        else
                        {
                            clearedNextGoalPrerequisites = false;
                            break;
                        }
                    }

                    if (clearedNextGoalPrerequisites)
                    {
                        m_isUsingFallbackGoal = false;
                        m_currentGoalIndex++;
                        m_currentGoal = m_goalsList[m_currentGoalIndex];
                    }
                    else
                    {
                        m_isUsingFallbackGoal = true;
                        m_currentGoal = m_fallbackGoal;
                    }
                }
                else
                {
                    m_isUsingFallbackGoal = true;
                    m_currentGoal = m_fallbackGoal;
                }
            }
        }
    }
}