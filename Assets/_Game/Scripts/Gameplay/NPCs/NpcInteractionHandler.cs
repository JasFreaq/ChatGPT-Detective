using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcInteractionHandler : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private int _randomIdleStates = 2;
    
    [SerializeField] private int _reactionStates = 3;
    
    private Animator _animator;

    private Interpolator<Quaternion> _interactionViewInterpolator;

    private int _baseIdleHash;

    private List<int> _randomIdleHashes = new List<int>();

    private List<int> _reactionHashes = new List<int>();

    private float _interactionViewInterpTime;

    private bool _disableRandomIdle;

    public float InteractionViewInterpTime { set => _interactionViewInterpTime = value; }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        AssignAnimationHashes();

        _interactionViewInterpolator = new Interpolator<Quaternion>(transform.rotation, Quaternion.identity,
            _interactionViewInterpTime, Quaternion.Slerp, InterruptRandomIdle, null, null,
            () => _disableRandomIdle = false);
    }

    private void Update()
    {
        if (!_disableRandomIdle)
            PlayRandomIdle();

        if (_interactionViewInterpolator.Interpolating)
        {
            transform.rotation = _interactionViewInterpolator.Update();
        }
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

    private void InterruptRandomIdle()
    {
        _disableRandomIdle = true;

        _animator.Play(_baseIdleHash);
    }

    public void ToggleInteractionView(bool enable, Vector3 viewPosition = new Vector3())
    {
        if (enable)
        {
            Vector3 lookDirection = viewPosition - transform.position;

            _interactionViewInterpolator.TargetVal = Quaternion.LookRotation(lookDirection);

            _interactionViewInterpolator.Toggle(true);
        }
        else
        {
            _interactionViewInterpolator.Toggle(false);
        }
    }
}
