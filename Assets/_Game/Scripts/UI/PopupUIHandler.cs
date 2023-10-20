using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Extensions;

public class PopupUIHandler : MonoBehaviour
{
    [SerializeField] private GameObject _popupHolder;

    [SerializeField] private Image _maleNPCImage;
    
    [SerializeField] private Image _femaleNPCImage;

    [SerializeField] private TextMeshProUGUI _nameText;

    [SerializeField] private GameObject _popupInteraction;

    private bool _popupEnabled;

    public void EnablePopup(int id, bool disregardInteraction = false)
    {
        if (!_popupEnabled)
        {
            NPCPopupDataHolder popupData = NPCTraceCache.Instance.GetPopupData(id);

            if (popupData != null) 
            {
                if (popupData.IsMale)
                {
                    _maleNPCImage.sprite = popupData.CharacterSprite;

                    _maleNPCImage.gameObject.SetActive(true);
                    _femaleNPCImage.gameObject.SetActive(false);
                }
                else
                {
                    _femaleNPCImage.sprite = popupData.CharacterSprite;

                    _maleNPCImage.gameObject.SetActive(false);
                    _femaleNPCImage.gameObject.SetActive(true);
                }

                _nameText.text = popupData.CharacterName;

                _popupInteraction.SetActive(!popupData.NoInteraction);

                _popupHolder.SetActive(true);

                _popupEnabled = true;
            }
        }
    }

    public void DisablePopup()
    {
        if (_popupEnabled)
        {
            _popupHolder.SetActive(false);

            _popupEnabled = false;
        }
    }
}
