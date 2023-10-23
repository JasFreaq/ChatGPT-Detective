using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using static UnityEngine.Rendering.DebugUI;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private InputActionReference _holdInputAction;
    [SerializeField] private InputActionReference _lookInputAction;

    [Header("Cinemachine")]

    [SerializeField] private CinemachineVirtualCamera _virtualCamera;

    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    [SerializeField] private GameObject _cinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")] 
    [SerializeField] private float _topClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")] 
    [SerializeField] private float _bottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    [SerializeField] private float _cameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")] 
    [SerializeField] private bool _lockCameraPosition = false;

    [SerializeField] private float _interactionViewCameraDistance = 1.2f;

    [SerializeField] private Vector3 _interactionViewCameraRotation = new Vector3(15, 340, 0);

    [Header("Mouse Cursor Settings")] [SerializeField]
    private bool _cursorLocked = true;

    private PlayerInput _playerInput;

    private Cinemachine3rdPersonFollow _cinemachine3rdPersonFollow;

    private Interpolator<float> _interactionViewDistanceInterpolator;
    
    private Interpolator<Quaternion> _interactionViewRotationInterpolator;
    
    private float _cinemachineTargetYaw;

    private float _cinemachineTargetPitch;

    private float _interactionViewInterpTime;

    private const float _threshold = 0.01f;

    private bool _rotate;
    
    private bool _disableInput;

    private Vector2 _lookInput;

    public float InteractionViewInterpTime { set => _interactionViewInterpTime = value; }

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();

        _cinemachine3rdPersonFollow = _virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
    }

    private void Start()
    {
        _cinemachineTargetYaw = _cinemachineCameraTarget.transform.rotation.eulerAngles.y;

        float initialDistance = _cinemachine3rdPersonFollow.CameraDistance;

        _interactionViewDistanceInterpolator = new Interpolator<float>(initialDistance, _interactionViewCameraDistance,
            _interactionViewInterpTime, Mathf.Lerp, () => _disableInput = true, null, null,
            () => _disableInput = false);

        _interactionViewRotationInterpolator = new Interpolator<Quaternion>(Quaternion.identity,
            Quaternion.Euler(_interactionViewCameraRotation),
            _interactionViewInterpTime, Quaternion.Slerp);
    }

    private void Update()
    {
        if (!_disableInput) 
        {
            ProcessInputs();
        }

        if (_interactionViewDistanceInterpolator.Interpolating)
        {
            _cinemachine3rdPersonFollow.CameraDistance = _interactionViewDistanceInterpolator.Update();
        }

        if (_interactionViewRotationInterpolator.Interpolating)
        {
            _cinemachineCameraTarget.transform.localRotation = _interactionViewRotationInterpolator.Update();
        }
    }

    private void ProcessInputs()
    {
        if (_holdInputAction.action.WasPressedThisFrame())
        {
            _rotate = true;
        }
        else if (_holdInputAction.action.WasReleasedThisFrame())
        {
            _rotate = false;
        }

        if (_rotate)
        {
            _lookInput = _lookInputAction.action.ReadValue<Vector2>();
        }
    }

    private void LateUpdate()
    {
        if (!_disableInput) 
        {
            CameraRotation();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(_cursorLocked);
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_lookInput.sqrMagnitude >= _threshold && !_lockCameraPosition)
        {
            bool isCurrentDeviceMouse = _playerInput.currentControlScheme == "KeyboardMouse";

            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = isCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _lookInput.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _lookInput.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _bottomClamp, _topClamp);

        // cinemachine will follow this target
        _cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + _cameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    public void ToggleInteractionView(bool enable)
    {
        _interactionViewDistanceInterpolator.Toggle(enable);

        if (enable)
        {
            _interactionViewRotationInterpolator.DefaultVal = _cinemachineCameraTarget.transform.localRotation;
        }

        _interactionViewRotationInterpolator.Toggle(enable);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}
