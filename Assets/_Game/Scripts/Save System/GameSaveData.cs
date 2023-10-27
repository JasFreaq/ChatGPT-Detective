namespace ChatGPT_Detective
{
    [System.Serializable]
    public class GameSaveData
    {
        public PlayerTransformSaveData m_playerTransformSave;

        public NpcHistorySaveData[] m_npcSaveData;

        public GameSaveData(PlayerTransformSaveData playerTransform, NpcHistorySaveData[] npcSaves)
        {
            m_playerTransformSave = playerTransform;
            m_npcSaveData = npcSaves;
        }
    }
}