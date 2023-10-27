using System.Collections;
using System.Collections.Generic;
using ChatGPT_Detective;
using UnityEngine;

namespace ChatGPT_Detective
{
    public class ChangeRegionVolume : Interactable
    {
        [SerializeField] private string m_interactionMessage;

        [SerializeField] private FaderUIHandler m_faderUIHandler;

        [SerializeField] Transform m_destinationTransform;

        private Transform m_playerTransform;

        private void Start()
        {
            m_playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        public override string GetInteractionMessage()
        {
            return m_interactionMessage;
        }

        public override void Interact()
        {
            m_faderUIHandler.ToggleFader(true, MovePlayer);
        }

        private void MovePlayer()
        {
            m_playerTransform.gameObject.SetActive(false);
            m_playerTransform.position = m_destinationTransform.position;
            m_playerTransform.rotation = m_destinationTransform.rotation;
            m_playerTransform.gameObject.SetActive(true);

            m_faderUIHandler.ToggleFader(false);
        }
    }
}
