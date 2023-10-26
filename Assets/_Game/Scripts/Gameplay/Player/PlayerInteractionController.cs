using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] private float _interactionViewInterpTime = 1f;
    [SerializeField] private UICoordinator _uiCoordinator;

    private PlayerLocomotionController _locomotionController;

    private PlayerCameraController _cameraController;

    private NpcInteractionHandler _currentNpc;

    private NpcPopupDataHolder _currentPopup;
    
    private bool _canInteractWithNpc;
    
    private bool _engagedConversation;

    private void Awake()
    {
        _locomotionController = GetComponent<PlayerLocomotionController>();
        _locomotionController.InteractionViewInterpTime = _interactionViewInterpTime;

        _cameraController = GetComponent<PlayerCameraController>();
        _cameraController.InteractionViewInterpTime = _interactionViewInterpTime;

        NpcInteractionHandler[] npcInteractionHandlers = FindObjectsByType<NpcInteractionHandler>(FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (NpcInteractionHandler interactionHandler in npcInteractionHandlers)
        {
            interactionHandler.InteractionViewInterpTime = _interactionViewInterpTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && _uiCoordinator.CanEngageConversation())
        {
            int id = other.gameObject.GetHashCode();

            _currentNpc = NpcDataCache.Instance.GetInteractionHandler(id);

            _currentPopup = NpcDataCache.Instance.GetPopupData(id);

            _uiCoordinator.TogglePopup(_currentPopup);
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other != null && _uiCoordinator.CanEngageConversation() && _currentNpc)
        {
            _uiCoordinator.TogglePopup(_currentPopup);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _uiCoordinator.TogglePopup();

        _currentNpc = null;
        _currentPopup = null;
    }

    private void OnInteract(InputValue value)
    {
        EngageConversation();
    }

    private void EngageConversation()
    {
        if (_currentNpc != null && !_engagedConversation && !_currentPopup.NoInteraction)
        {
            _uiCoordinator.TogglePopup();
            _uiCoordinator.ToggleChat(_currentNpc.gameObject.GetHashCode());

            _locomotionController.ToggleInteractionView(true, _currentNpc.transform.position);
            _cameraController.ToggleInteractionView(true);

            _currentNpc.ToggleInteractionView(true, transform.position);

            _engagedConversation = true;
        }
    }
    
    public void DisengageConversation()
    {
        if (_engagedConversation)
        {
            _uiCoordinator.TogglePopup(_currentPopup);
            _uiCoordinator.ToggleChat();

            _locomotionController.ToggleInteractionView(false);
            _cameraController.ToggleInteractionView(false);

            _currentNpc.ToggleInteractionView(false);

            _engagedConversation = false;
        }
    }
}
