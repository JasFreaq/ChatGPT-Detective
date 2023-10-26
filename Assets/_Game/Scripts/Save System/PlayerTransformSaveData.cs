using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SerializableVector
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class PlayerTransformSaveData
{
    public SerializableVector position;
    public SerializableVector rotation;
}
