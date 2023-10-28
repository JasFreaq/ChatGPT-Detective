namespace ChatGPT_Detective
{
    [System.Serializable]
    public class GameSaveData
    {
        public TransformSaveData mPlayerTransformSave;
        
        public TransformSaveData mCameraRootTransformSave;

        public NpcHistorySaveData[] mNpcSaveData;

        public GameSaveData(TransformSaveData playerTransform, TransformSaveData cameraRootTransform, NpcHistorySaveData[] npcSaves)
        {
            mPlayerTransformSave = playerTransform;
            mCameraRootTransformSave = cameraRootTransform;
            mNpcSaveData = npcSaves;
        }
    }
}