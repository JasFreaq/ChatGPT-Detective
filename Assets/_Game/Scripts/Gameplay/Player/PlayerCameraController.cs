using System.Collections;
using System.Collections.Generic;
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
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    [SerializeField]
    private GameObject _cinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")] [SerializeField]
    private float _topClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")] [SerializeField]
    private float _bottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    [SerializeField]
    private float _cameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")] [SerializeField]
    private bool _lockCameraPosition = false;

    [Header("Mouse Cursor Settings")] [SerializeField]
    private bool _cursorLocked = true;

    private PlayerInput _playerInput;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    private const float _threshold = 0.01f;

    private Vector2 _lookInput;

    private bool _rotate;
    
    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        _cinemachineTargetYaw = _cinemachineCameraTarget.transform.rotation.eulerAngles.y;
    }

    private void Update()
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
        CameraRotation();
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
