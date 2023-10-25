using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NpcHistoryButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _nameText;
    
    public void SetupButton(Action buttonCallback, string name)
    {
        UnityAction unityAction = new UnityAction(buttonCallback);
        _button.onClick.AddListener(unityAction);

        _nameText.text = name;
    }
}
