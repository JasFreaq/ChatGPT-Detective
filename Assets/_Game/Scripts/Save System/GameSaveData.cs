using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public PlayerTransformSaveData playerTransformSave;

    public NpcHistorySaveData[] npcSaveData;

    public GameSaveData(PlayerTransformSaveData playerTransform, NpcHistorySaveData[] npcSaves)
    {
        playerTransformSave = playerTransform;
        npcSaveData = npcSaves;
    }
}
