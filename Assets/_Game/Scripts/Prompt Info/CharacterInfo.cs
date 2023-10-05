using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChatGPT_Detective
{
    [CreateAssetMenu(fileName = "New Character Info", menuName = "Prompt Info/Character Info", order = 1)]
    public class CharacterInfo :
        ScriptableObject
    {
        #region Member Variables

        [SerializeField] private string _characterName;

        [SerializeField] [TextArea(5, 15)] private string _personalityInfo;

        [SerializeField] [TextArea(5, 15)] private string _behaviourInfo;

        [SerializeField] [TextArea(5, 15)] private string _motivationsInfo;

        [SerializeField] [TextArea(5, 15)] private string _instructionsInfo;

        #endregion

        #region Functions

        public string GeCharacterInfo()
        {
            return $"Name: {_characterName}\n\n" +
                   $"Personality:\n{_personalityInfo}\n\n" +
                   $"Behaviour:\n{_behaviourInfo}\n\n" +
                   $"Motivations:\n{_motivationsInfo}\n\n" +
                   $"Character Instructions: {_instructionsInfo}\n\n";
        }

        #endregion
    }
}
