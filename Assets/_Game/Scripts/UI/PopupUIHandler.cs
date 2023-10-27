using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChatGPT_Detective
{
    public class PopupUIHandler : MonoBehaviour
    {
        [SerializeField] private GameObject m_popupHolder;

        [SerializeField] private Image m_maleNPCImage;

        [SerializeField] private Image m_femaleNPCImage;

        [SerializeField] private TextMeshProUGUI m_nameText;

        [SerializeField] private GameObject m_popupNpcInfo;

        [SerializeField] private GameObject m_popupInteraction;

        [SerializeField] private TextMeshProUGUI m_interactionText;

        [SerializeField] private string m_npcInteractionMessage = "Press Interact to Talk";

        private bool m_isPopupEnabled;

        public void EnableNpcPopup(NpcPopupDataHolder popupData)
        {
            if (!m_isPopupEnabled)
            {
                if (popupData.IsMale)
                {
                    m_maleNPCImage.sprite = popupData.CharacterSprite;

                    m_maleNPCImage.gameObject.SetActive(true);
                    m_femaleNPCImage.gameObject.SetActive(false);
                }
                else
                {
                    m_femaleNPCImage.sprite = popupData.CharacterSprite;

                    m_maleNPCImage.gameObject.SetActive(false);
                    m_femaleNPCImage.gameObject.SetActive(true);
                }

                m_nameText.text = popupData.CharacterName;

                m_popupNpcInfo.SetActive(true);

                if (!popupData.NoInteraction)
                {
                    m_interactionText.text = m_npcInteractionMessage;
                    m_popupInteraction.SetActive(true);
                }
                else
                {
                    m_popupInteraction.SetActive(false);
                }

                m_popupHolder.SetActive(true);

                m_isPopupEnabled = true;
            }
        }
        
        public void EnableEnvironmentPopup(string popupMessage)
        {
            if (!m_isPopupEnabled)
            {
                m_popupNpcInfo.SetActive(false);

                m_interactionText.text = popupMessage;
                m_popupInteraction.SetActive(true);

                m_popupHolder.SetActive(true);

                m_isPopupEnabled = true;
            }
        }

        public void DisablePopup()
        {
            if (m_isPopupEnabled)
            {
                m_popupHolder.SetActive(false);

                m_isPopupEnabled = false;
            }
        }
    }
}