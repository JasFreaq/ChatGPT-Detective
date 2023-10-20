using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] private PopupUIHandler _popupUIHandler;

    [SerializeField] private ChatUIHandler _chatUIHandler;

    private int _currentNpcId;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            _currentNpcId = other.gameObject.GetHashCode();
            
            _popupUIHandler.EnablePopup(_currentNpcId);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _popupUIHandler.DisablePopup();
    }

    private void OnInteract(InputValue value)
    {
        if (_popupUIHandler.PopupEnabled)
            _popupUIHandler.DisablePopup();

        _chatUIHandler.EnableChat(_currentNpcId);
    }
}
