using System.IO;
using UnityEngine;

namespace ChatGPT_Detective
{
    public class SaveSystem : MonoBehaviour
    {
        private static SaveSystem s_instance;

        public static SaveSystem Instance
        {
            get
            {
                if (!s_instance)
                    s_instance = FindFirstObjectByType<SaveSystem>();

                return s_instance;
            }
        }

        private string m_savePath;

        private Transform m_playerTransform;

        private NpcPrompter[] m_npcs;

        private void Awake()
        {
            SaveSystem[] handlers =
                FindObjectsByType<SaveSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (handlers.Length > 1)
            {
                Destroy(gameObject);
            }
            else
            {
                s_instance = this;
            }

            m_savePath = Path.Combine(Application.persistentDataPath, "saveGame.sav");

            m_playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

            m_npcs = FindObjectsByType<NpcPrompter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        private void Start()
        {
            LoadGameData();
        }

        public void SavePlayerData()
        {
            PlayerTransformSaveData playerTransformSave = new PlayerTransformSaveData(m_playerTransform);

            NpcHistorySaveData[] npcHistorySaves = new NpcHistorySaveData[m_npcs.Length];

            for (int i = 0, l = m_npcs.Length; i < l; i++)
            {
                npcHistorySaves[i] = new NpcHistorySaveData(m_npcs[i]);
            }

            GameSaveData gameSaveData = new GameSaveData(playerTransformSave, npcHistorySaves);

            string json = JsonUtility.ToJson(gameSaveData);

            File.WriteAllText(m_savePath, json);
        }

        private void LoadGameData()
        {
            if (File.Exists(m_savePath))
            {
                string json = File.ReadAllText(m_savePath);

                GameSaveData gameSaveData = JsonUtility.FromJson<GameSaveData>(json);

                PlayerTransformSaveData playerTransformSave = gameSaveData.m_playerTransformSave;

                m_playerTransform.position = playerTransformSave.mPosition.ToVector();
                m_playerTransform.rotation = Quaternion.Euler(playerTransformSave.mRotation.ToVector());

                NpcHistorySaveData[] npcHistorySaves = gameSaveData.m_npcSaveData;

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
            else
            {
                Debug.Log("No Save Data Found");
            }
        }
    }
}