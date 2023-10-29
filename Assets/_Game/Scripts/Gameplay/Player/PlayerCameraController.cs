using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChatGPT_Detective
{
    public class PlayerCameraController : MonoBehaviour
    {
        [SerializeField] private InputActionReference m_holdInputAction;
        [SerializeField] private InputActionReference m_lookInputAction;

        [Header("Cinemachine")]
        [SerializeField] private CinemachineVirtualCamera m_virtualCamera;
        
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        [SerializeField] private GameObject m_cinemachineCameraTarget;
        
        [Tooltip("How far in degrees can you move the camera up")]
        [SerializeField] private float m_topClamp = 70.0f;
        
        [Tooltip("How far in degrees can you move the camera down")]
        [SerializeField] private float m_bottomClamp = -30.0f;
        
        [Tooltip("Additional degrees to override the camera. Useful for fine-tuning camera position when locked")]
        [SerializeField] private float m_cameraAngleOverride = 0.0f;
        
        [Tooltip("For locking the camera position on all axes")]
        
        [SerializeField] private bool m_lockCameraPosition = false;
        
        [SerializeField] private float m_interactionViewCameraDistance = 1.2f;
        
        [SerializeField] private Vector3 m_interactionViewCameraRotation = new Vector3(15, 340, 0);

        [Header("Mouse Cursor Settings")]
        [SerializeField] private bool m_cursorLocked = true;

        private PlayerInput m_playerInput;

        private Cinemachine3rdPersonFollow m_cinemachine3rdPersonFollow;

        private Interpolator<float> m_interactionViewDistanceInterpolator;

        private Interpolator<Quaternion> m_interactionViewRotationInterpolator;

        private float m_cinemachineTargetYaw;

        private float m_cinemachineTargetPitch;

        private float m_interactionViewInterpTime;

        private const float m_threshold = 0.01f;

        private bool m_rotate;

        private bool m_isInputDisabled;

        private Vector2 m_lookInput;

        public float InteractionViewInterpTime
        {
            set => m_interactionViewInterpTime = value;
        }
        
        private void Awake()
        {
            m_playerInput = GetComponent<PlayerInput>();
            m_cinemachine3rdPersonFollow = m_virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        }

        private void Start()
        {
            m_cinemachineTargetYaw = m_cinemachineCameraTarget.transform.rotation.eulerAngles.y;

            float initialDistance = m_cinemachine3rdPersonFollow.CameraDistance;

            m_interactionViewDistanceInterpolator = new Interpolator<float>(initialDistance,
                m_interactionViewCameraDistance,
                m_interactionViewInterpTime, Mathf.Lerp, () => m_isInputDisabled = true,
                onReachedDefault: () => m_isInputDisabled = false);

            m_interactionViewRotationInterpolator = new Interpolator<Quaternion>(Quaternion.identity,
                Quaternion.Euler(m_interactionViewCameraRotation),
                m_interactionViewInterpTime, Quaternion.Slerp);
        }

        private void Update()
        {
            if (!m_isInputDisabled)
            {
                ProcessInputs();
            }

            if (m_interactionViewDistanceInterpolator.Interpolating)
            {
                m_cinemachine3rdPersonFollow.CameraDistance = m_interactionViewDistanceInterpolator.Update();
            }

            if (m_interactionViewRotationInterpolator.Interpolating)
            {
                m_cinemachineCameraTarget.transform.localRotation = m_interactionViewRotationInterpolator.Update();
            }
        }

        private void LateUpdate()
        {
            if (!m_isInputDisabled)
            {
                CameraRotation();
            }
        }

        private void ProcessInputs()
        {
            if (m_holdInputAction.action.WasPressedThisFrame())
            {
                m_rotate = true;
            }
            else if (m_holdInputAction.action.WasReleasedThisFrame())
            {
                m_rotate = false;
            }

            if (m_rotate)
            {
                m_lookInput = m_lookInputAction.action.ReadValue<Vector2>();
            }
            else
            {
                m_lookInput = Vector2.zero;
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(m_cursorLocked);
        }

        private void CameraRotation()
        {
            if (m_lookInput.sqrMagnitude >= m_threshold && !m_lockCameraPosition)
            {
                bool isCurrentDeviceMouse = m_playerInput.currentControlScheme == "KeyboardMouse";
                float deltaTimeMultiplier = isCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                m_cinemachineTargetYaw += m_lookInput.x * deltaTimeMultiplier;
                m_cinemachineTargetPitch += m_lookInput.y * deltaTimeMultiplier;
            }

            m_cinemachineTargetYaw = ClampAngle(m_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            m_cinemachineTargetPitch = ClampAngle(m_cinemachineTargetPitch, m_bottomClamp, m_topClamp);

            m_cinemachineCameraTarget.transform.rotation = Quaternion.Euler(
                m_cinemachineTargetPitch + m_cameraAngleOverride,
                m_cinemachineTargetYaw, 0.0f);
        }

        public void ToggleInteractionView(bool enable)
        {
            m_interactionViewDistanceInterpolator.Toggle(enable);

            if (enable)
            {
                m_interactionViewRotationInterpolator.DefaultVal = m_cinemachineCameraTarget.transform.localRotation;
            }

            m_interactionViewRotationInterpolator.Toggle(enable);
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
}
