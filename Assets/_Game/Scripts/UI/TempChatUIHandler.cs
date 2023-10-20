using System.Collections;
using System.Collections.Generic;
using ChatGPT_Detective;
using OpenAI.Chat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TempChatUIHandler : MonoBehaviour
{
    [SerializeField] private Button submitButton;

    [SerializeField] private TMP_InputField inputField;

    [SerializeField] private RectTransform contentArea;

    [SerializeField] private ScrollRect scrollView;

    [SerializeField] NpcPrompter _npcPrompter;

    private void Start()
    {
        submitButton.onClick.AddListener(SendReply);
    }

    private void OnEnable()
    {
        GPTPromptIntegrator.Instance.RegisterOnResponseReceived(UpdateChat);
    }

    private void OnDisable()
    {
        GPTPromptIntegrator.Instance.DeregisterOnResponseReceived(UpdateChat);
    }

    private void SendReply()
    {
        _npcPrompter.ProcessPromptRequest(inputField.text);
    }

    private TextMeshProUGUI AddNewTextMessageContent()
    {
        var textObject = new GameObject($"Message_{contentArea.childCount + 1}");
        textObject.transform.SetParent(contentArea, false);
        var textMesh = textObject.AddComponent<TextMeshProUGUI>();
        textMesh.fontSize = 24;
        textMesh.enableWordWrapping = true;
        return textMesh;
    }

    private void UpdateChat(Message response)
    {
        var assistantMessageContent = AddNewTextMessageContent();
        assistantMessageContent.text = $"Assistant: {response.Content}";
    }
}
