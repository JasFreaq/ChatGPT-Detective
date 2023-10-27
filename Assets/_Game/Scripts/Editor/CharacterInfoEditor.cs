using UnityEditor;
using UnityEngine;

namespace ChatGPT_Detective
{
    [CustomEditor(typeof(CharacterInfo))]
    public class CharacterInfoEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            CharacterInfo characterInfo = (CharacterInfo)target;
            
            DrawDefaultInspector();
            
            if (GUILayout.Button("Generate Goal Ids"))
            {
                characterInfo.GenerateGoalIds();
            }
        }
    }
}