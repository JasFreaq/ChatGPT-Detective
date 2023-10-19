using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAnimationHandler : MonoBehaviour
{
    [SerializeField] private int _randomIdleStates = 2;
    
    [SerializeField] private int _reactionStates = 3;
    
    private Animator _animator;

    private int _baseIdleHash;

    private List<int> _randomIdleHashes = new List<int>();
    private List<int> _reactionHashes = new List<int>();

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        AssignAnimationHashes();
    }

    private void Update()
    {
        PlayRandomIdle();
    }

    private void AssignAnimationHashes()
    {
        _baseIdleHash = Animator.StringToHash("Idle0");

        for (int i = 0; i < _randomIdleStates; i++)
        {
            int hash = Animator.StringToHash($"Idle{i + 1}");
            _randomIdleHashes.Add(hash);
        }

        for (int i = 0; i < _reactionStates; i++)
        {
            int hash = Animator.StringToHash($"Reaction{i + 1}");
            _reactionHashes.Add(hash);
        }
    }

    private void PlayRandomIdle()
    {
        if (Random.Range(0, 3600) < 1)
        {
            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);

            if (currentState.shortNameHash == _baseIdleHash)
            {
                int randomIndex = Random.Range(0, _randomIdleStates);

                _animator.Play(_randomIdleHashes[randomIndex]);
            }
        }
    }
    
    public void PlayRandomReaction()
    {
        int randomIndex = Random.Range(0, _reactionStates);

        _animator.Play(_reactionHashes[randomIndex]);
    }
}
