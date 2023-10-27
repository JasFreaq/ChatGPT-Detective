using UnityEngine;

namespace ChatGPT_Detective
{
    public class NpcPopupDataHolder : MonoBehaviour
    {
        [SerializeField] private Sprite m_characterSprite;

        [SerializeField] private string m_characterName;

        [SerializeField] private bool m_isMale = true;

        [SerializeField] private bool m_noInteraction;

        public Sprite CharacterSprite => m_characterSprite;

        public string CharacterName => m_characterName;

        public bool IsMale => m_isMale;

        public bool NoInteraction => m_noInteraction;
    }
}