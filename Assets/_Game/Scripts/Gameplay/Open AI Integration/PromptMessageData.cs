using ChatGPT_Detective;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChatGPT_Detective
{
    public struct PromptMessageData
    {
        private CharacterInfo _charInfo;

        private string _newPrompt;

        private HistoryData _historyData;

        private GoalInfo _npcCurrentGoal;

        public CharacterInfo CharInfo => _charInfo;

        public string NewPrompt => _newPrompt;

        public HistoryData HistoryData => _historyData;

        public GoalInfo NpcCurrentGoal => _npcCurrentGoal;

        public PromptMessageData(CharacterInfo charInfo, string newPrompt, HistoryData historyData,
            GoalInfo npcCurrentGoal)
        {
            _charInfo = charInfo;
            _newPrompt = newPrompt;
            _historyData = historyData;
            _npcCurrentGoal = npcCurrentGoal;
        }
    }
}
