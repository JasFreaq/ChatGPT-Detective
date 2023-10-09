using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChatGPT_Detective
{
    [CreateAssetMenu(fileName = "New Character Info", menuName = "Prompt Info/Character Info", order = 1)]
    public class CharacterInfo : ScriptableObject
    {
        #region Member Variables

        [SerializeField] private string _characterName;

        [SerializeField] [TextArea(5, 15)] private string _characterInfo;

        [SerializeField] [TextArea(5, 15)] private string _characterInstructions;

        #endregion

        #region Functions

        public string GetCharacterInfo()
        {
            return $"<character>\n{_characterInfo}\n</character>";
        }

        public string GetCharacterInstructions()
        {
            return $"{_characterInstructions}\n\n###\n\n";
        }

        #endregion
    }
}
