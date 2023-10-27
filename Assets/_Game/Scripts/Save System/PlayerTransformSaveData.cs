using UnityEngine;

namespace ChatGPT_Detective
{
    [System.Serializable]
    public struct SerializableVector
    {
        public float mX;
        public float mY;
        public float mZ;

        public SerializableVector(Vector3 v)
        {
            mX = v.x;
            mY = v.y;
            mZ = v.z;
        }

        public Vector3 ToVector()
        {
            return new Vector3(mX, mY, mZ);
        }
    }

    [System.Serializable]
    public class PlayerTransformSaveData
    {
        public SerializableVector mPosition;

        public SerializableVector mRotation;

        public PlayerTransformSaveData(Transform transform)
        {
            mPosition = new SerializableVector(transform.position);
            mRotation = new SerializableVector(transform.rotation.eulerAngles);
        }
    }
}