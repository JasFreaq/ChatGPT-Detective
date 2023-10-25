using System;
using System.Collections;
using System.Collections.Generic;
using ChatGPT_Detective;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookUIHandler : MonoBehaviour
{
    [SerializeField] private float _bookOpenTime = 1.2f;
    [SerializeField] private GameObject _bookUI;

    [Header("Interpolation")] 
    [SerializeField] private GameObject _bookButton;
    [SerializeField] private RectTransform _bookInterpIcon;
    [SerializeField] private RectTransform _sourceTransform;
    [SerializeField] private RectTransform _targetTransform;

    [Header("Animation")]
    [SerializeField] private Animator _bookAnimator;
    [SerializeField] private string _openState = "Open";
    [SerializeField] private string _closeState = "Close";
    [SerializeField] private string _forwardFlipState = "Flip Forward";
    [SerializeField] private string _reverseFlipState = "Flip Reverse";
    [SerializeField] private float _openAnimationTime = 0.5f;
    [SerializeField] private float _flipAnimationTime = 0.5f;

    [Header("UI")]
    [SerializeField] private NpcHistoryButton _npcHistoryButtonPrefab;
    [SerializeField] private GameObject _npcButtonsPanel;
    [SerializeField] private TextMeshProUGUI _firstPageText;
    [SerializeField] private TextMeshProUGUI _secondPageText;
    [SerializeField] private GameObject _npcHistoryPanel;
    [SerializeField] private GameObject[] _flipLeftButton;
    [SerializeField] private GameObject[] _flipRightButton;

    private NpcPrompter[] _npcs;
    private NpcHistoryButton[] _npcButtons;

    private Interpolator<float> _bookAnimationInterpolator;

    private bool _isBookOpen;

    private int _animHashOpen;
    private int _animHashClose;
    private int _animHashForwardFlip;
    private int _animHashReverseFlip;
    
    public bool IsBookOpen => _isBookOpen;

    private void Awake()
    {
        _npcs = FindObjectsByType<NpcPrompter>(FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        Array.Sort(_npcs, new NpcPrompter.NpcPrompterComparer());
    }

    private void Start()
    {
        _bookAnimationInterpolator = new Interpolator<float>(0f, 1f, _bookOpenTime, Mathf.Lerp, null, null,
            OpenBook, DisableBook);

        AssignAnimationHashes();

        SetupNpcHistoryUI();
    }

    private void AssignAnimationHashes()
    {
        _animHashOpen = Animator.StringToHash(_openState);
        _animHashClose = Animator.StringToHash(_closeState);
        _animHashForwardFlip = Animator.StringToHash(_forwardFlipState);
        _animHashReverseFlip = Animator.StringToHash(_reverseFlipState);
    }

    private void SetupNpcHistoryUI()
    {
        _npcButtons = new NpcHistoryButton[_npcs.Length];

        for (int i = 0; i < _npcs.Length; i++)
        {
            NpcHistoryButton npcHistoryButton = Instantiate(_npcHistoryButtonPrefab, _npcButtonsPanel.transform);

            int index = i;
            npcHistoryButton.SetupButton(() => { EnableNpcHistoryLayout(index); }, _npcs[i].CharInfo.CharacterName);

            _npcButtons[i] = npcHistoryButton;
        }
    }

    private void Update()
    {
        if (_bookAnimationInterpolator.Interpolating)
        {
            float t = _bookAnimationInterpolator.Update();

            _bookInterpIcon.position = Vector3.Lerp(_sourceTransform.position, _targetTransform.position, t);
            _bookInterpIcon.sizeDelta = Vector2.Lerp(_sourceTransform.sizeDelta, _targetTransform.sizeDelta, t);
        }
    }

    public void EnableBook()
    {
        _bookInterpIcon.gameObject.SetActive(true);
        _bookButton.SetActive(false);

        _isBookOpen = true;

        _bookAnimationInterpolator.Toggle(true);
    }

    private void DisableBook()
    {
        _bookInterpIcon.gameObject.SetActive(false);
        _bookButton.SetActive(true);

        _isBookOpen = false;
    }

    private void OpenBook()
    {
        _bookInterpIcon.gameObject.SetActive(false);
        
        StartCoroutine(OpenBookRoutine());
    }
    
    public void CloseBook()
    {
        _bookUI.SetActive(false);
        
        _npcHistoryPanel.SetActive(false);
        foreach (GameObject obj in _flipLeftButton)
            obj.SetActive(false);
        foreach (GameObject obj in _flipRightButton)
            obj.SetActive(false);

        StartCoroutine(CloseBookRoutine());
    }

    private IEnumerator OpenBookRoutine()
    {
        _bookAnimator.gameObject.SetActive(true);

        _bookAnimator.Play(_animHashOpen);

        yield return new WaitForSeconds(_openAnimationTime);

        yield return FlipBookAnimationRoutine(1);

        _bookAnimator.gameObject.SetActive(false);

        EnableNpcButtonsLayout();

        _bookUI.SetActive(true);
    }
    
    private IEnumerator CloseBookRoutine()
    {
        _bookAnimator.gameObject.SetActive(true);

        yield return FlipBookAnimationRoutine(1, true);
        
        _bookAnimator.Play(_animHashClose);

        yield return new WaitForSeconds(_openAnimationTime);

        _bookAnimator.gameObject.SetActive(false);

        _bookInterpIcon.gameObject.SetActive(true);

        _bookAnimationInterpolator.Toggle(false);
    }

    private IEnumerator FlipBookAnimationRoutine(int repeat = 0, bool close = false)
    {
        _bookAnimator.Play(close ? _animHashReverseFlip : _animHashForwardFlip);

        yield return new WaitForSeconds(_flipAnimationTime * (repeat + 1));
    }

    private void EnableNpcButtonsLayout()
    {
        for (int i = 0; i < _npcs.Length; i++)
        {
            bool interacted = _npcs[i].History.Count > 0;
            _npcButtons[i].gameObject.SetActive(interacted);
        }
        
        _npcButtonsPanel.SetActive(true);
    }
    
    private void EnableNpcHistoryLayout(int index)
    {
        _npcButtonsPanel.SetActive(false);

        IReadOnlyList<DialogueChunk> history = _npcs[index].History;

        string name = _npcs[index].CharInfo.CharacterName;
        string historyText = "";

        foreach (DialogueChunk chunk in history)
        {
            historyText += $"Detective: {chunk.Prompt.Content}\n";
            historyText += $"{name}: {chunk.Response.Content}\n";
        }

        _firstPageText.text = historyText;

        StartCoroutine(EnableNpcHistoryLayoutRoutine(index));
    }
    
    private IEnumerator EnableNpcHistoryLayoutRoutine(int index)
    {
        _bookAnimator.gameObject.SetActive(true);

        yield return FlipBookAnimationRoutine();

        _bookAnimator.gameObject.SetActive(false);

        string historyText = _firstPageText.text;

        _npcHistoryPanel.SetActive(true);

        foreach (GameObject obj in _flipLeftButton)
            obj.SetActive(true);

        yield return new WaitForEndOfFrame();

        if (_firstPageText.textInfo.pageCount > 1)
        {
            int firstPageLastIndex = _firstPageText.textInfo.pageInfo[0].lastCharacterIndex;
            _secondPageText.text = historyText.Substring(firstPageLastIndex + 1, historyText.Length - firstPageLastIndex - 1);

            _secondPageText.gameObject.SetActive(true);

            if (_firstPageText.textInfo.pageCount > 2)
            {
                foreach (GameObject obj in _flipRightButton)
                    obj.SetActive(true);
            }
        }
        else
        {
            _secondPageText.gameObject.SetActive(false);
        }
    }

    public void FlipPageLeft()
    {
        _npcHistoryPanel.SetActive(false);

        StartCoroutine(FlipPageLeftRoutine());
    }

    public void FlipPageRight()
    {
        _npcHistoryPanel.SetActive(false);

        StartCoroutine(FlipPageRightRoutine());
    }

    private IEnumerator FlipPageLeftRoutine()
    {
        _bookAnimator.gameObject.SetActive(true);

        yield return FlipBookAnimationRoutine(0, true);

        _bookAnimator.gameObject.SetActive(false);

        if (_firstPageText.pageToDisplay == 1)
        {
            foreach (GameObject obj in _flipLeftButton)
                obj.SetActive(false);
         
            EnableNpcButtonsLayout();
        }
        else
        {
            string historyText = _firstPageText.text;

            _npcHistoryPanel.SetActive(true);

            int nextPage = _firstPageText.pageToDisplay - 2;
            _firstPageText.pageToDisplay = nextPage;

            int firstPageLastIndex = _firstPageText.textInfo.pageInfo[nextPage - 1].lastCharacterIndex;
            _secondPageText.text =
                historyText.Substring(firstPageLastIndex + 1, historyText.Length - firstPageLastIndex - 1);

            _secondPageText.gameObject.SetActive(true);

            foreach (GameObject obj in _flipRightButton)
                obj.SetActive(true);
        }
    }
    
    private IEnumerator FlipPageRightRoutine()
    {
        _bookAnimator.gameObject.SetActive(true);

        yield return FlipBookAnimationRoutine();

        _bookAnimator.gameObject.SetActive(false);

        string historyText = _firstPageText.text;

        _npcHistoryPanel.SetActive(true);

        int nextPage = _firstPageText.pageToDisplay + 2;
        _firstPageText.pageToDisplay = nextPage;

        if (_firstPageText.textInfo.pageCount > nextPage)
        {
            int firstPageLastIndex = _firstPageText.textInfo.pageInfo[nextPage - 1].lastCharacterIndex;
            _secondPageText.text = historyText.Substring(firstPageLastIndex + 1, historyText.Length - firstPageLastIndex - 1);

            _secondPageText.gameObject.SetActive(true);

            foreach (GameObject obj in _flipRightButton)
                obj.SetActive(_firstPageText.textInfo.pageCount > nextPage + 1);
        }
        else
        {
            _secondPageText.gameObject.SetActive(false);

            foreach (GameObject obj in _flipRightButton)
                obj.SetActive(false);
        }
    }
}
