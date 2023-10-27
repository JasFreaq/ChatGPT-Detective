using OpenAI;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace ChatGPT_Detective
{
    public class UICoordinator : MonoBehaviour
    {
        private static UICoordinator s_instance;

        public static UICoordinator Instance
        {
            get
            {
                if (!s_instance)
                    s_instance = FindFirstObjectByType<UICoordinator>();

                return s_instance;
            }
        }

        private void Awake()
        {
            UICoordinator[] handlers = FindObjectsByType<UICoordinator>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            if (handlers.Length > 1)
            {
                Destroy(gameObject);
            }
            else
            {
                s_instance = this;
            }
        }

        [Header("Dialogue UI")] 
        [SerializeField] private PopupUIHandler m_popupUIHandler;

        [SerializeField] private ChatUIHandler m_chatUIHandler;

        [Header("HUD UI")] 
        [SerializeField] private BookUIHandler m_bookUIHandler;

        public void EnableNpcPopup(NpcPopupDataHolder popupData)
        {
            if (popupData != null)
            {
                m_popupUIHandler.EnableNpcPopup(popupData);
            }
        }

        public void EnableEnvironmentPopup(Interactable interactable)
        {
            if (interactable != null) 
            {
                m_popupUIHandler.EnableEnvironmentPopup(interactable.GetInteractionMessage());
            }
        }

        public void DisablePopup()
        {
            m_popupUIHandler.DisablePopup();
        }

        public void ToggleChat(int id = 0)
        {
            if (id != 0)
            {
                m_chatUIHandler.EnableChat(id);
            }
            else
            {
                m_chatUIHandler.DisableChat();
            }
        }

        public bool CanInteractWithEnvironment()
        {
            return !m_bookUIHandler.IsBookOpen;
        }
    }
}