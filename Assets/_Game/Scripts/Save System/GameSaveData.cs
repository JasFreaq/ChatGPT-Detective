namespace ChatGPT_Detective
{
    [System.Serializable]
    public class GameSaveData
    {
        public TransformSaveData mPlayerTransformSave;
        
        public TransformSaveData mCameraRootTransformSave;

        public NpcSaveData[] mNpcSaveData;

        public GoalsSaveData mGoalsSaveData;

        public GameSaveData(TransformSaveData playerTransform, TransformSaveData cameraRootTransform, NpcSaveData[] npcSaves, GoalsSaveData goalsSaveData)
        {
            mPlayerTransformSave = playerTransform;
            mCameraRootTransformSave = cameraRootTransform;

            mNpcSaveData = npcSaves;

            mGoalsSaveData = goalsSaveData;
        }
    }
}