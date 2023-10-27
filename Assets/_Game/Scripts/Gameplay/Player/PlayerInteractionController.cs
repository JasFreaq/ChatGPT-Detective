using UnityEngine;
using UnityEngine.InputSystem;

namespace ChatGPT_Detective
{
    public class PlayerInteractionController : MonoBehaviour
    {
        [SerializeField] private float m_interactionViewInterpTime = 1f;
        
        private PlayerLocomotionController m_locomotionController;

        private PlayerCameraController m_cameraController;

        private NpcInteractionHandler m_currentNpc;

        private Interactable m_currentInteractable;

        private NpcPopupDataHolder m_currentPopup;
        
        private bool m_isEngagedInConversation;

        private void Awake()
        {
            m_locomotionController = GetComponent<PlayerLocomotionController>();
            m_locomotionController.InteractionViewInterpTime = m_interactionViewInterpTime;

            m_cameraController = GetComponent<PlayerCameraController>();
            m_cameraController.InteractionViewInterpTime = m_interactionViewInterpTime;

            NpcInteractionHandler[] npcInteractionHandlers = FindObjectsByType<NpcInteractionHandler>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (NpcInteractionHandler interactionHandler in npcInteractionHandlers)
            {
                interactionHandler.InteractionViewInterpolationTime = m_interactionViewInterpTime;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other != null && UICoordinator.Instance.CanInteractWithEnvironment())
            {
                int id = other.gameObject.GetHashCode();

                m_currentNpc = NpcDataCache.Instance.GetInteractionHandler(id);

                m_currentPopup = NpcDataCache.Instance.GetPopupData(id);

                if (m_currentPopup != null)
                {
                    UICoordinator.Instance.EnableNpcPopup(m_currentPopup);
                }
                else
                {
                    m_currentInteractable = EnvironmentCache.Instance.GetInteractable(id);

                    if (m_currentInteractable != null)
                    {
                        UICoordinator.Instance.EnableEnvironmentPopup(m_currentInteractable);
                    }
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other != null && UICoordinator.Instance.CanInteractWithEnvironment())
            {
                if (m_currentPopup != null) 
                {
                    UICoordinator.Instance.EnableNpcPopup(m_currentPopup);
                }
                else if (m_currentInteractable != null)
                {
                    UICoordinator.Instance.EnableEnvironmentPopup(m_currentInteractable);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            m_currentNpc = null;
            m_currentPopup = null;
         
            UICoordinator.Instance.DisablePopup();
        }

        private void OnInteract(InputValue value)
        {
            if (UICoordinator.Instance.CanInteractWithEnvironment()) 
            {
                if (m_currentNpc != null)
                {
                    EngageConversation();
                }
                else if (m_currentInteractable != null)
                {
                    m_currentInteractable.Interact();
                }
            }
        }

        private void EngageConversation()
        {
            if (!m_isEngagedInConversation && !m_currentPopup.NoInteraction)
            {
                UICoordinator.Instance.DisablePopup();
                UICoordinator.Instance.ToggleChat(m_currentNpc.gameObject.GetHashCode());

                m_locomotionController.ToggleInteractionView(true, m_currentNpc.transform.position);
                m_cameraController.ToggleInteractionView(true);

                m_currentNpc.ToggleInteractionView(true, transform.position);

                m_isEngagedInConversation = true;
            }
        }

        public void DisengageConversation()
        {
            if (m_isEngagedInConversation)
            {
                UICoordinator.Instance.EnableNpcPopup(m_currentPopup);
                UICoordinator.Instance.ToggleChat();

                m_locomotionController.ToggleInteractionView(false);
                m_cameraController.ToggleInteractionView(false);

                m_currentNpc.ToggleInteractionView(false);

                m_isEngagedInConversation = false;
            }
        }
    }
}
