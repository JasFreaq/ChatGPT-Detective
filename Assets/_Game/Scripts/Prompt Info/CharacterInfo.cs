using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChatGPT_Detective
{
    [CreateAssetMenu(fileName = "New Character Info", menuName = "Prompt Info/Character Info", order = 1)]
    public class CharacterInfo : ScriptableObject
    {
        #region Member Variables

        [SerializeField] private int m_characterId;

        [SerializeField] private string m_characterName;

        [SerializeField][TextArea(5, 15)] private string m_characterInfo;

        [SerializeField][TextArea(5, 15)] private string m_characterInstructions;

        [SerializeField] private List<GoalInfo> m_characterGoals = new List<GoalInfo>();

        [SerializeField][TextArea(5, 15)] private string m_characterFallbackGoal;

        #endregion

        public int CharId => m_characterId;

        public string CharacterName => m_characterName;

        public string CharInfo => $"<character>\n{m_characterInfo}\n</character>";

        public string CharInstructions => $"{m_characterInstructions}\n\n###\n\n";

        public IReadOnlyList<GoalInfo> CharGoals => m_characterGoals;

        public string CharFallbackGoal => m_characterFallbackGoal;

        #region Functions

        public void GenerateGoalIds()
        {
            for (int i = 0, n = m_characterGoals.Count; i < n; i++)
            {
                GoalInfo goal = m_characterGoals[i];
                int id = Convert.ToInt32($"{m_characterId}{i + 1}");

                m_characterGoals[i] = new GoalInfo(id, goal);
            }
        }

        #endregion
    }
}