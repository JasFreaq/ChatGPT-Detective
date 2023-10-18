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
    [Tooltip("Move speed of the character in m/s")] 
    [SerializeField] private float _moveSpeed = 2.0f;

    [Tooltip("How fast the character turns to face movement direction")] 
    [Range(0.0f, 0.3f)] 
    [SerializeField] private float _rotationSmoothTime = 0.12f;
    
    [SerializeField] private float _movementBlendTime = 0.8f;

    [SerializeField] private AudioClip[] _footstepAudioClips;

    [Range(0, 1)] 
    [SerializeField] private float _footstepAudioVolume = 0.5f;

    [Header("Animation")]
    [SerializeField] private string _speedAnimParameter = "Speed";
    [SerializeField] private string _horizontalAxisParameter = "HorizontalAxis";
    
    // Player
    private float _targetRotation;
    private float _rotationVelocity;
    
    // Animation Hashes
    private int _animHashSpeed;
    private int _animHashHorizontalAxis;
    
    private Animator _animator;
    private CharacterController _controller;
    private GameObject _mainCamera;

    private Vector2 _moveInput;
    private Vector2 _lastMoveInput;
    private Vector2 _lastMoveDirection;

    private bool _movementBlending;

    private float _movementBlendTimer;
    private float _movementBlendSrc;
    private float _movementBlendTarget;

    private float _moveTimer;

    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        AssignAnimationHashes();
    }

    private void Update()
    {
        Vector2 currentMoveInput = _moveInputAction.action.ReadValue<Vector2>();

        bool beganMove = _lastMoveInput == Vector2.zero && currentMoveInput != Vector2.zero;
        bool endedMove = _lastMoveInput != Vector2.zero && currentMoveInput == Vector2.zero;

        if (beganMove)
        {
            _movementBlending = true;
            _movementBlendSrc = 0f;
            _movementBlendTarget = 1f;
        }
        else if (endedMove)
        {
            _movementBlending = true;
            _movementBlendSrc = 1f;
            _movementBlendTarget = 0f;

            _lastMoveDirection = _lastMoveInput.normalized;
        }
        else
        {
            _moveInput = currentMoveInput;
        }

        if (_movementBlending)
        {
            _movementBlendTimer += Time.deltaTime;

            float t = _movementBlendTimer / _movementBlendTime;

            if (t > 1f)
            {
                _movementBlendTimer = 0f;

                _movementBlending = false;
             
                _moveInput = _moveInput.normalized * _movementBlendTarget;
            }
            else
            {
                float currentMag = Mathf.Lerp(_movementBlendSrc, _movementBlendTarget, t);

                if (currentMoveInput != Vector2.zero) 
                {
                    _moveInput = currentMoveInput.normalized * currentMag;
                }
                else
                {
                    _moveInput = _lastMoveDirection * currentMag;
                }
                
            }
        }

        _lastMoveInput = _moveInput;

        Move();
    }
    
    private void AssignAnimationHashes()
    {
        _animHashSpeed = Animator.StringToHash(_speedAnimParameter);
        _animHashHorizontalAxis = Animator.StringToHash(_horizontalAxisParameter);
    }

    private void Move()
    {
        float targetSpeed = _moveInput == Vector2.zero ? 0f : _moveSpeed;

        // a reference to the players current horizontal velocity
        float currentSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speed;
        float inputMagnitude = _moveInput.magnitude;
        
        // accelerate or decelerate to target speed
        speed = _moveSpeed * inputMagnitude;
        // round speed to 3 decimal places
        speed = Mathf.Round(speed * 1000f) / 1000f;
        

        // normalise input direction
        Vector3 inputDirection = new Vector3(_moveInput.x, 0.0f, _moveInput.y).normalized;
        
        // if there is a move input rotate player when the player is moving
        if (_moveInput != Vector2.zero)
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
        //_animator.SetFloat(_animHashHorizontalAxis, _moveInput.x);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (_footstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, _footstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(_footstepAudioClips[index], transform.TransformPoint(_controller.center), _footstepAudioVolume);
            }
        }
    }
}
