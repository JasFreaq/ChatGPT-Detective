using ChatGPT_Detective;
using OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class SaveSystem : MonoBehaviour
{
    private static SaveSystem _instance;

    public static SaveSystem Instance
    {
        get
        {
            if (!_instance)
                _instance = FindFirstObjectByType<SaveSystem>();

            return _instance;
        }
    }

    private string _savePath;

    private Transform _player;

    private NpcPrompter[] _npcs;

    private void Awake()
    {
        SaveSystem[] handlers = FindObjectsByType<SaveSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (handlers.Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }

        _savePath = Path.Combine(Application.persistentDataPath, "saveGame.sav");

        _player = GameObject.FindGameObjectWithTag("Player")?.transform;

        _npcs = FindObjectsByType<NpcPrompter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    private void Start()
    {
        LoadGameData();
    }

    public void SavePlayerData()
    {
        PlayerTransformSaveData playerTransformSave = new PlayerTransformSaveData(_player);

        NpcHistorySaveData[] npcHistorySaves = new NpcHistorySaveData[_npcs.Length];

        for (int i = 0, l = _npcs.Length; i < l; i++)
        {
            npcHistorySaves[i] = new NpcHistorySaveData(_npcs[i]);
        }

        GameSaveData gameSaveData = new GameSaveData(playerTransformSave, npcHistorySaves);

        string json = JsonUtility.ToJson(gameSaveData);

        File.WriteAllText(_savePath, json);
    }

    private void LoadGameData()
    {
        if (File.Exists(_savePath))
        {
            string json = File.ReadAllText(_savePath);
            
            GameSaveData gameSaveData = JsonUtility.FromJson<GameSaveData>(json);

            PlayerTransformSaveData playerTransformSave = gameSaveData.playerTransformSave;

            _player.position = playerTransformSave.position.ToVector();
            _player.rotation = Quaternion.Euler(playerTransformSave.rotation.ToVector());

            NpcHistorySaveData[] npcHistorySaves = gameSaveData.npcSaveData;

            foreach (NpcHistorySaveData npcSaveData in npcHistorySaves)
            {
                NpcPrompter npc = null;
                for (int i = 0, l = _npcs.Length; i < l; i++)
                {
                    if (npcSaveData.charId == _npcs[i].CharInfo.CharId)
                    {
                        npc = _npcs[i];
                        break;
                    }
                }

                npc.InitialiseFromSaveData(npcSaveData.promptHistory);
            }
        }
        else
        {
            Debug.Log("No Save Data Found");
        }
    }
}
