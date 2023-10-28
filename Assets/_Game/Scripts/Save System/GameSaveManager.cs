using UnityEngine;

namespace ChatGPT_Detective
{
    public class GameSaveManager : MonoBehaviour
    {
        private static GameSaveManager s_instance;

        public static GameSaveManager Instance
        {
            get
            {
                if (!s_instance)
                    s_instance = FindFirstObjectByType<GameSaveManager>();

                return s_instance;
            }
        }

        private Transform m_playerTransform;
        
        private Transform m_cameraRootTransform;

        private NpcPrompter[] m_npcs;

        private void Awake()
        {
            GameSaveManager[] handlers =
                FindObjectsByType<GameSaveManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (handlers.Length > 1)
            {
                Destroy(gameObject);
            }
            else
            {
                s_instance = this;
            }

            m_playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            
            m_cameraRootTransform = GameObject.FindGameObjectWithTag("CinemachineTarget")?.transform;

            m_npcs = FindObjectsByType<NpcPrompter>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (SaveSystem.DoesSaveGameExist() && SaveSystem.IsGameContinuing())
            {
                LoadGameData();
            }
        }
        
        public void SaveGameData()
        {
            TransformSaveData playerTransformSave = new TransformSaveData(m_playerTransform);
            
            TransformSaveData cameraRootTransformSave = new TransformSaveData(m_cameraRootTransform);

            NpcHistorySaveData[] npcHistorySaves = new NpcHistorySaveData[m_npcs.Length];

            for (int i = 0, l = m_npcs.Length; i < l; i++)
            {
                npcHistorySaves[i] = new NpcHistorySaveData(m_npcs[i]);
            }

            GameSaveData gameSaveData =
                new GameSaveData(playerTransformSave, cameraRootTransformSave, npcHistorySaves);

            SaveSystem.SavePlayerData(gameSaveData);
        }

        private void LoadGameData()
        {
            GameSaveData gameSaveData = SaveSystem.LoadGameData();

            TransformSaveData playerTransformSave = gameSaveData.mPlayerTransformSave;
            m_playerTransform.position = playerTransformSave.mPosition.ToVector();
            m_playerTransform.rotation = Quaternion.Euler(playerTransformSave.mRotation.ToVector());
            
            TransformSaveData cameraRootTransformSave = gameSaveData.mCameraRootTransformSave;
            m_cameraRootTransform.position = cameraRootTransformSave.mPosition.ToVector();
            m_cameraRootTransform.rotation = Quaternion.Euler(cameraRootTransformSave.mRotation.ToVector());
            
            NpcHistorySaveData[] npcHistorySaves = gameSaveData.mNpcSaveData;

            foreach (NpcHistorySaveData npcSaveData in npcHistorySaves)
            {
                NpcPrompter npc = null;
                for (int i = 0, l = m_npcs.Length; i < l; i++)
                {
                    if (npcSaveData.mCharId == m_npcs[i].CharInfo.CharId)
                    {
                        npc = m_npcs[i];
                        break;
                    }
                }

                npc.InitialiseFromSaveData(npcSaveData.mPromptHistory);
            }
        }
    }
}