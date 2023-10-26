using System;
using System.Collections;
using System.Collections.Generic;
using ChatGPT_Detective;
using OpenAI.Chat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.HID;

public class ChatUIHandler : MonoBehaviour
{
    [SerializeField] private GameObject _playerInputPanel;

    [SerializeField] private GameObject _npcResponsePanel;

    [SerializeField] private TMP_InputField _playerInputField;

    [SerializeField] private TextMeshProUGUI _responseText;

    [SerializeField] private TextMeshProUGUI _npcNameText;

    [SerializeField] private Button _npcResponseConfirmationButton;

    [SerializeField] private float _streamInterpolationSpeed = 0.1f;

    private NpcPrompter _npcPrompter;

    private string _streamResponseTargetString;

    private Coroutine _streamResponseCoroutine;

    private void Start()
    {
        _npcResponseConfirmationButton.onClick.AddListener(ConfirmNpcResponse);
    }

    private void OnEnable()
    {
        GPTPromptIntegrator.Instance.RegisterOnResponseStreaming(StreamNpcResponse);
        GPTPromptIntegrator.Instance.RegisterOnResponseReceived(UpdateNpcResponse);
    }

    private void OnDisable()
    {
        GPTPromptIntegrator.Instance.DeregisterOnResponseStreaming(StreamNpcResponse);
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

            _streamResponseTargetString = string.Empty;
            _responseText.text = "...";

            _playerInputPanel.SetActive(false);
            _npcResponsePanel.SetActive(true);

            _npcResponseConfirmationButton.gameObject.SetActive(false);
        }
    }

    public void ClearPlayerInput()
    {
        _playerInputField.text = "";
    }

    private void StreamNpcResponse(string response)
    {
        if (string.IsNullOrEmpty(_streamResponseTargetString))
        {
            _responseText.text = string.Empty;

            _streamResponseTargetString = response;
            _streamResponseCoroutine = StartCoroutine(StringInterpolationRoutine());
        }
        else
        {
            _streamResponseTargetString += response;

            _streamResponseCoroutine ??= StartCoroutine(StringInterpolationRoutine());
        }
    }
    
    private void UpdateNpcResponse(Message _)
    {
        _npcResponseConfirmationButton.gameObject.SetActive(true);
    }

    private void ConfirmNpcResponse()
    {
        if (_responseText.text.Length < _streamResponseTargetString.Length)
        {
            if (_streamResponseCoroutine != null)
            {
                StopCoroutine(_streamResponseCoroutine);
                _streamResponseCoroutine = null;
            }
            
            _responseText.text = _streamResponseTargetString;
        }
        else
        {
            _playerInputField.text = string.Empty;

            _playerInputPanel.SetActive(true);
            _npcResponsePanel.SetActive(false);
        }
    }

    private IEnumerator StringInterpolationRoutine()
    {
        int maxLength = _streamResponseTargetString.Length;
        int i = _responseText.text.Length;

        while (i < maxLength)
        {
            string interpolatedString = _responseText.text;

            interpolatedString += _streamResponseTargetString[i];

            _responseText.text = interpolatedString;

            yield return new WaitForSeconds(_streamInterpolationSpeed);

            i++;
            maxLength = _streamResponseTargetString.Length;
        }

        _streamResponseCoroutine = null;
    }
}
