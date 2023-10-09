using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChatGPT_Detective
{
    [CreateAssetMenu(fileName = "New World Context Info", menuName = "Prompt Info/World Context Info", order = 0)]
    public class WorldContextInfo : ScriptableObject
    {
        #region Member Variables

        [SerializeField] [TextArea(5, 15)] private string _plotInfo;

        [SerializeField] [TextArea(5, 15)] private string _settingInfo;

        #endregion

        #region Functions

        public string GetWorldInfo()
        {
            return $"<plot>\n{_plotInfo}\n</plot>\n\n###\n\n" +
                   $"<setting>\n{_settingInfo}\n</setting>\n\n###";
        }

        #endregion
    }
}
