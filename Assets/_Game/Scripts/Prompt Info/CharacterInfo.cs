using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChatGPT_Detective
{
    [CreateAssetMenu(fileName = "New Character Info", menuName = "Prompt Info/Character Info", order = 1)]
    public class CharacterInfo : ScriptableObject
    {
        #region Member Variables

        [SerializeField] private int _characterId;
        
        [SerializeField] private string _characterName;

        [SerializeField] [TextArea(5, 15)] private string _characterInfo;

        [SerializeField] [TextArea(5, 15)] private string _characterInstructions;

        [SerializeField] private List<GoalInfo> _characterGoals = new List<GoalInfo>();

        #endregion

        public int CharId => _characterId;

        public string CharacterName => _characterName;

        public string CharInfo => $"<character>\n{_characterInfo}\n</character>";

        public string CharInstructions => $"{_characterInstructions}\n\n###\n\n";

        public IReadOnlyList<GoalInfo> CharGoals => _characterGoals;

        #region Functions
        
        public void GenerateGoalIds()
        {
            for (int i = 0, n = _characterGoals.Count; i < n; i++)
            {
                GoalInfo goal = _characterGoals[i];
                int id = Convert.ToInt32($"{_characterId}{i + 1}");

                _characterGoals[i] = new GoalInfo(id, goal);
            }
        }

        #endregion
    }
}
