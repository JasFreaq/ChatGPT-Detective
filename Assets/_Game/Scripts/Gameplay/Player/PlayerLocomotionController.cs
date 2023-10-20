using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotionController : MonoBehaviour
{
    [SerializeField] private InputActionReference _moveInputAction;

    [Header("Player")] 
    [Tooltip("ProcessLocomotion speed of the character in m/s")] 
    [SerializeField] private float _moveSpeed = 2.0f;

    [Tooltip("How fast the character turns to face movement direction")] 
    [Range(0.0f, 0.3f)] 
    [SerializeField] private float _rotationSmoothTime = 0.12f;
    
    [SerializeField] private float _locomotionBlendTime = 1.6f;

    [SerializeField] private AudioClip[] _footstepAudioClips;

    [Header("Animation")]
    [SerializeField] private string _speedAnimParameter = "Speed";
    [SerializeField] private string _startedWalkingParameter = "StartedWalking";
    
    // Player
    private float _targetRotation;
    private float _rotationVelocity;
    
    // Animation Hashes
    private int _animHashSpeed;
    private int _animHashStartedWalking;
    
    private Animator _animator;
    private CharacterController _controller;
    private GameObject _mainCamera;
    
    private AudioSource _footstepAudioSource;
    
    private Vector2 _moveInputToProcess;
    private Vector2 _lastMoveInput;
    private Vector2 _lastMoveDirection;

    private bool _isLocomotionBlending;

    private float _locomotionBlendTimer;

    private float _locomotionBlendSrc;

    private float _locomotionBlendTarget;

    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        
        _footstepAudioSource = GetComponentInChildren<AudioSource>();
    }

    private void Start()
    {
        AssignAnimationHashes();
    }

    private void Update()
    {
        HandleLocomotionBlending();

        ProcessLocomotion();
    }

    private void AssignAnimationHashes()
    {
        _animHashSpeed = Animator.StringToHash(_speedAnimParameter);
        _animHashStartedWalking = Animator.StringToHash(_startedWalkingParameter);
    }

    private void HandleLocomotionBlending()
    {
        Vector2 currentMoveInput = _moveInputAction.action.ReadValue<Vector2>();

        bool beganMove = _lastMoveInput == Vector2.zero && currentMoveInput != Vector2.zero;
        bool endedMove = _lastMoveInput != Vector2.zero && currentMoveInput == Vector2.zero;

        if (beganMove || endedMove)
        {
            _animator.SetBool(_animHashStartedWalking, beganMove);

            if (_isLocomotionBlending)
            {
                _locomotionBlendTimer = _locomotionBlendTime - _locomotionBlendTimer;
            }

            if (beganMove)
            {
                _locomotionBlendSrc = 0f;
                _locomotionBlendTarget = 1f;
            }
            else
            {
                _locomotionBlendSrc = 1f;
                _locomotionBlendTarget = 0f;

                _lastMoveDirection = _lastMoveInput.normalized;
            }

            _isLocomotionBlending = true;
        }
        else
        {
            _moveInputToProcess = currentMoveInput;
        }

        if (_isLocomotionBlending)
        {
            _locomotionBlendTimer += Time.deltaTime;

            float t = _locomotionBlendTimer / _locomotionBlendTime;

            if (t > 1f)
            {
                _locomotionBlendTimer = 0f;

                _isLocomotionBlending = false;

                _moveInputToProcess = _moveInputToProcess.normalized * _locomotionBlendTarget;
            }
            else
            {
                float currentMag = Mathf.Lerp(_locomotionBlendSrc, _locomotionBlendTarget, t);

                if (currentMoveInput != Vector2.zero)
                {
                    _moveInputToProcess = currentMoveInput.normalized * currentMag;
                }
                else
                {
                    _moveInputToProcess = _lastMoveDirection * currentMag;
                }
            }
        }

        _lastMoveInput = currentMoveInput;
    }

    private void ProcessLocomotion()
    {
        float targetSpeed = _moveInputToProcess == Vector2.zero ? 0f : _moveSpeed;

        // a reference to the players current horizontal velocity
        float currentSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
        
        float inputMagnitude = _moveInputToProcess.magnitude;

        // accelerate or decelerate to target speed
        float speed = _moveSpeed * inputMagnitude;
        // round speed to 3 decimal places
        speed = Mathf.Round(speed * 1000f) / 1000f;
        

        // normalise input direction
        Vector3 inputDirection = new Vector3(_moveInputToProcess.x, 0.0f, _moveInputToProcess.y).normalized;
        
        if (_moveInputToProcess != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              _mainCamera.transform.eulerAngles.y;

            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                _rotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // move the player
        _controller.Move(targetDirection.normalized * (speed * Time.deltaTime));

        _animator.SetFloat(_animHashSpeed, speed);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (_footstepAudioClips.Length > 0 && !_footstepAudioSource.isPlaying)
            {
                int index = Random.Range(0, _footstepAudioClips.Length);
                _footstepAudioSource.clip = _footstepAudioClips[index];
                
                _footstepAudioSource.Play();
            }
        }
    }
}
