namespace ChatGPT_Detective
{
    public struct PromptMessageData
    {
        private CharacterInfo m_charInfo;

        private string m_newPrompt;

        private HistoryData m_historyData;

        private GoalInfo m_npcCurrentGoal;

        public CharacterInfo CharInfo => m_charInfo;

        public string NewPrompt => m_newPrompt;

        public HistoryData HistoryData => m_historyData;

        public GoalInfo NpcCurrentGoal => m_npcCurrentGoal;

        public PromptMessageData(CharacterInfo charInfo, string newPrompt, HistoryData historyData,
            GoalInfo npcCurrentGoal)
        {
            m_charInfo = charInfo;
            m_newPrompt = newPrompt;
            m_historyData = historyData;
            m_npcCurrentGoal = npcCurrentGoal;
        }
    }
}
