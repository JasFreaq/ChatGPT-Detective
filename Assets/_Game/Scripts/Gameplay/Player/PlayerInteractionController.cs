using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    [Header("Tracing")]
    [SerializeField] private float _traceRange = 100f;

    [SerializeField] private LayerMask _traceLayer;

    private PopupUIHandler _popupUIHandler;

    private void Start()
    {
        _popupUIHandler = FindFirstObjectByType<PopupUIHandler>();

        if (_popupUIHandler == null )
            Debug.Log("There is no Popup UI Handler present in the scene.");
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            _popupUIHandler.EnablePopup(other.GetHashCode());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _popupUIHandler.DisablePopup();
    }
}
