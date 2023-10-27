using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ChatGPT_Detective
{
    public class NpcHistoryButton : MonoBehaviour
    {
        [SerializeField] private Button m_button;

        [SerializeField] private TextMeshProUGUI m_nameText;

        public void SetupButton(Action buttonCallback, string name)
        {
            UnityAction unityAction = new UnityAction(buttonCallback);
            m_button.onClick.AddListener(unityAction);

            m_nameText.text = name;
        }
    }
}