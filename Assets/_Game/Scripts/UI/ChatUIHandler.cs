using System;
using System.Collections;
using System.Collections.Generic;
using ChatGPT_Detective;
using OpenAI.Chat;
using TMPro;
using UnityEngine;

public class ChatUIHandler : MonoBehaviour
{
    [SerializeField] private GameObject _playerInputPanel;

    [SerializeField] private GameObject _npcResponsePanel;

    [SerializeField] private TMP_InputField _playerInputField;

    [SerializeField] private TextMeshProUGUI _responseText;

    [SerializeField] private TextMeshProUGUI _npcNameText;

    private NpcPrompter _npcPrompter;

    private void OnEnable()
    {
        GPTPromptIntegrator.Instance.RegisterOnResponseReceived(UpdateNpcResponse);
    }

    private void OnDisable()
    {
        GPTPromptIntegrator.Instance.DeregisterOnResponseReceived(UpdateNpcResponse);
    }

    public void EnableChat(int id)
    {
        NpcPrompter prompter = NpcDataCache.Instance.GetPrompter(id);

        if (prompter != null)
        {
            _npcPrompter = prompter;
            _npcPrompter.enabled = true;

            _npcNameText.text = _npcPrompter.CharInfo.CharacterName;
            
            _playerInputPanel.SetActive(true);
        }
    }

    public void DisableChat()
    {
        _playerInputPanel.SetActive(false);
        _npcResponsePanel.SetActive(false);

        _npcPrompter.enabled = false;
        _npcPrompter = null;
    }

    public void SendPlayerInput()
    {
        if (_playerInputField.text != string.Empty) 
        {
            _npcPrompter.ProcessPromptRequest(_playerInputField.text);

            _responseText.text = "...";

            _playerInputPanel.SetActive(false);
            _npcResponsePanel.SetActive(true);
        }
    }

    public void ClearPlayerInput()
    {
        _playerInputField.text = "";
    }

    private void UpdateNpcResponse(Message response)
    {
        _responseText.text = response.Content;
    }

    public void ConfirmNpcResponse()
    {
        _playerInputField.text = "";

        _playerInputPanel.SetActive(true);
        _npcResponsePanel.SetActive(false);
    }
}
