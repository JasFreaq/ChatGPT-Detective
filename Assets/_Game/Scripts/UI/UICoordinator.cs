using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICoordinator : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private PopupUIHandler _popupUIHandler;
    [SerializeField] private ChatUIHandler _chatUIHandler;

    [Header("HUD UI")]
    [SerializeField] private BookUIHandler _bookUIHandler;

    public void TogglePopup(int id = 0)
    {
        if (id != 0)
        {
            _popupUIHandler.EnablePopup(id);
        }
        else
        {
            _popupUIHandler.DisablePopup();
        }
    }
    
    public void ToggleChat(int id = 0)
    {
        if (id != 0)
        {
            _chatUIHandler.EnableChat(id);
        }
        else
        {
            _chatUIHandler.DisableChat();
        }
    }

    public bool CanEngageConversation()
    {
        return !_bookUIHandler.IsBookOpen;
    }
}
