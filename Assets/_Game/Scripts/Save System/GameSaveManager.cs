using System.Collections;
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

        [SerializeField] private string m_playerTag = "Player";
        
        [SerializeField] private string m_cameraTag = "CinemachineTarget";

        private Transform m_playerTransform;
        
        private Transform m_cameraRootTransform;

        private NpcPrompter[] m_npcs;

        private SystemGoalsManager m_goalsManager;

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

            m_playerTransform = GameObject.FindGameObjectWithTag(m_playerTag)?.transform;
            
            m_cameraRootTransform = GameObject.FindGameObjectWithTag(m_cameraTag)?.transform;

            m_npcs = FindObjectsByType<NpcPrompter>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            m_goalsManager = FindObjectOfType<SystemGoalsManager>(true);
        }

        private void Start()
        {
            if (SaveSystem.DoesSaveGameExist() && SaveSystem.IsGameContinuing())
            {
                LoadGameData();
            }
        }

        public void SaveGameData()
        {
            TransformSaveData playerTransformSave = new TransformSaveData(m_playerTransform);
            
            TransformSaveData cameraRootTransformSave = new TransformSaveData(m_cameraRootTransform);

            NpcSaveData[] npcHistorySaves = new NpcSaveData[m_npcs.Length];

            for (int i = 0, l = m_npcs.Length; i < l; i++)
            {
                npcHistorySaves[i] = new NpcSaveData(m_npcs[i]);
            }

            GoalsSaveData goalsSave = new GoalsSaveData(m_goalsManager.ClearedGoalsLog);

            GameSaveData gameSaveData =
                new GameSaveData(playerTransformSave, cameraRootTransformSave, npcHistorySaves, goalsSave);

            SaveSystem.SavePlayerData(gameSaveData);
        }

        private void LoadGameData()
        {
            GameSaveData gameSaveData = SaveSystem.LoadGameData();

            TransformSaveData playerTransformSave = gameSaveData.mPlayerTransformSave;
            m_playerTransform.position = playerTransformSave.mPosition.ToVector();
            m_playerTransform.rotation = Quaternion.Euler(playerTransformSave.mRotation.ToVector());
            
            TransformSaveData cameraRootTransformSave = gameSaveData.mCameraRootTransformSave;
            m_cameraRootTransform.rotation = Quaternion.Euler(cameraRootTransformSave.mRotation.ToVector());
            
            NpcSaveData[] npcHistorySaves = gameSaveData.mNpcSaveData;

            foreach (NpcSaveData npcSaveData in npcHistorySaves)
            {
                if (npcSaveData.mPromptHistory.Length > 0) 
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

                    npc.InitialiseFromSaveData(npcSaveData.mPromptHistory, npcSaveData.mGoalSave);
                }
            }

            m_goalsManager.LoadGoalLog(gameSaveData.mGoalsSaveData);
        }
    }
}