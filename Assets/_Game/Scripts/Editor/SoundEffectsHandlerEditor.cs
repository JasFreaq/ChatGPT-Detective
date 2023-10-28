using UnityEditor;

namespace ChatGPT_Detective
{
    [CustomEditor(typeof(SoundEffectsHandler))]
    public class SoundEffectsHandlerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SoundEffectsHandler soundEffectsHandler = (SoundEffectsHandler)target;
            
            DrawDefaultInspector();
            
            if (soundEffectsHandler.AreClipsPlayedFromRandomTime)
            {
                soundEffectsHandler.ClipRandomTimeBuffer = EditorGUILayout.FloatField("Clip Random Time Buffer", soundEffectsHandler.ClipRandomTimeBuffer);
            }
        }
    }
}