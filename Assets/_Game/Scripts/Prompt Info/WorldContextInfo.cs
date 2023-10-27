using UnityEngine;

namespace ChatGPT_Detective
{
    [CreateAssetMenu(fileName = "New World Context Info", menuName = "Prompt Info/World Context Info", order = 0)]
    public class WorldContextInfo : ScriptableObject
    {
        #region Member Variables

        [SerializeField] private string m_plotInfo;

        [SerializeField] private string m_settingInfo;

        #endregion

        #region Functions

        public string GetWorldInfo()
        {
            return $"<plot>\n{m_plotInfo}\n</plot>\n\n###\n\n" +
                   $"<setting>\n{m_settingInfo}\n</setting>\n\n###";
        }

        #endregion
    }
}