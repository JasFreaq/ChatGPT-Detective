using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SerializableVector
{
    public float x;
    public float y;
    public float z;

    public SerializableVector(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3 ToVector()
    {
        return new Vector3(x, y, z);
    }
}

[System.Serializable]
public class PlayerTransformSaveData
{
    public SerializableVector position;
    public SerializableVector rotation;

    public PlayerTransformSaveData(Transform transform)
    {
        position = new SerializableVector(transform.position);
        rotation = new SerializableVector(transform.rotation.eulerAngles);
    }
}
